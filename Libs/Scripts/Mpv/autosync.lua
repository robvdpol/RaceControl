-- autosync.lua version 0.9.0 2021-05-22
-- Public domain
-- No warranty

-- PURPOSE
-- For use with MPV (https://mpv.io) only when streaming *LIVE* video.
-- Automatically adjusts playback rate to remain a fixed amount behind real-time stream.
-- The real-time of a stream is estimated from the maximum value of available cache.

-- CONTEXT
-- Designed for use with F1 Race Control:  https://github.com/robvdpol/RaceControl
-- and MPV:  https://mpv.io/manual/stable/#lua-scripting
-- and Lua:  https://www.lua.org/manual
--
-- USAGE
-- Copy this file to a folder on your machine and then, in F1 Race Control,
-- add the following to the its "Additional MPV parameters":
--     --script=C:\FOLDERNAME\autosync.lua
-- By default autosync will try to lag 30.0 seconds behind real time.
-- It's inactive initially, until activated by pressing shift-A.
-- Add the option as follows to choose a different lag and be turned on initially:
--    --script-opts=autosync-lag=20.0
-- Different lags can be set for use with different video resolutions (pixels high):
--    --script-opts=autosync-lag=30.0,autosync-lag-1080=28.0
--
-- The current lag can be increased or decreased in 0.1 second steps
-- using the "(" and ")" keys.

require 'mp.msg'

autoSync = false -- Are we active?
maxSyncTime = 4.0 -- target seconds to resync
offsetMax = -math.huge -- maximum seconds the end of the cache has been ahead of real time
interval = 1.0 -- how often we update the playback speed in seconds
maxSpeed = 5.0 -- maximum playback speed factor
minSpeed = 1.0 / maxSpeed
slowSyncIntervals = 2.0 -- how many intervals should we take to do final, slow syncing
slowSyncRate = 0.1 -- how much to speed up or slow down for final, slow syncing
slowSyncErr = (slowSyncIntervals) * slowSyncRate * interval -- use only slow syncing within this error limit
slowSyncTarget = (slowSyncIntervals - 0.5) * slowSyncRate * interval -- aim fast syncing just inside the slow sync limit
minFast = 1.0 + slowSyncRate
maxSlow = 1.0 - slowSyncRate
syncErr = slowSyncRate * interval * 0.5 -- tolerated sync offset seconds

lastSpeed = 1.0 -- remember the last playback speed
syncOff = nil -- when do we plan to be synced
avErr = 0 -- running average to correct drift

-- continuously monitor the cache duration to maintain offsetMax
mp.observe_property("demuxer-cache-duration", "number", function(prop, c)
	if autoSync and c ~= nil then
		local p = mp.get_property("time-pos")
		if p ~= nil then
			local off = p + c - mp.get_time()
			if off > offsetMax then
--				if off > 60.0 then -- more than a minute ahead: this isn't a live stream
--					toggleAutoSync()
--				else
					mp.msg.info(string.format("CACHE: %+1.2f = %+1.2f", off - offsetMax, off))
					offsetMax = off
					lastSpeed = 1.0 -- forces sync speed recalculation
--				end
			end
		end
	end
end)

-- set offsetTarget when we first discover stream height in pixels
mp.observe_property("height", "number", function(prop, height)
	if height ~= nil and height > 0 and height < 10000 then
		local propName=string.format("autosync-lag-%1d", height)
		offsetTarget = mp.get_opt(string.format("autosync-lag-%1d", height)) -- height-specific lag
		if offsetTarget == nil then
			offsetTarget = mp.get_opt("autosync-lag") -- non-specific lag
		end
		if offsetTarget ~= nil then
			offsetTarget = offsetTarget + 0.0
			if offsetTarget > 120.0 then
				offsetTarget = 120.0
			elseif offsetTarget < 3.0 then
				offsetTarget = 3.0
			end
			autoSync = true -- start with autosync on if the lag is set in the options
		else
			offsetTarget = 30.0 -- default lag
		end
		mp.msg.info(string.format("HEIGHT: %1d; LAG: %1.2f", height, offsetTarget))
	end
end)

mp.add_periodic_timer(interval, function()
	local now = mp.get_time()                   -- current real time in seconds to arbitrary epoch
	local playPos = mp.get_property("time-pos") -- position of current playback in seconds
	if autoSync and playPos ~= nil then -- has playback begun?
		local playOff = playPos - now       -- seconds play is ahead of real time
		local err = offsetMax - playOff - offsetTarget
		local speed = 1.0
		local syncTime = maxSyncTime
		if err > syncErr then -- we're behind: speed up
			if err > slowSyncErr then
				if lastSpeed > 1.0 then -- continuing adjustment
					syncTime = math.max(interval, syncOff - now)
				end
				speed = 1.0 + (err - slowSyncTarget) / syncTime
				if speed > maxSpeed then
					speed = maxSpeed
					syncOff = (err - slowSyncTarget) / (speed - 1.0) + now
				else
					syncOff = syncTime + now
				end
			else
				speed = minFast
				syncOff = math.ceil(err / (speed - 1.0) / interval) * interval
				speed = 1.0 + err / syncOff -- sync in an exact number of intervals
				syncOff = syncOff + now
			end
		elseif err < -syncErr then -- we're ahead: slow down
			if err < -slowSyncErr then
				if lastSpeed < 1.0 then -- continuing adjustment
					syncTime = math.max(interval, syncOff - now)
				end
				speed = 1.0 + (err + slowSyncTarget) / syncTime
				if speed < minSpeed then
					speed = minSpeed
					syncOff = (err + slowSyncTarget) / (speed - 1.0) + now
				else
					syncOff = syncTime + now
				end
			else
				speed = maxSlow
				syncOff = math.ceil(err / (speed - 1.0) / interval) * interval
				speed = 1.0 + err / syncOff -- sync in an exact number of intervals
				syncOff = syncOff + now
			end
		else -- close enough: normal speed
			avErr = (19.0 * avErr + err) * 0.05 -- crude running average
			syncOff = now
		end
		if math.abs(speed - lastSpeed) < 0.02 then
			if speed == 1.0 then
				speed = 1.0 + avErr / interval
				if speed < maxSlow or speed > minFast or speed > 0.98 and speed < 1.02 then
					speed = 1.0
				else
					mp.set_property("speed", speed) -- rare fine adjustment
				end
			else
				speed = lastSpeed
			end
		else
			mp.set_property("speed", speed)
		end
		lastSpeed = speed
		mp.msg.info(string.format("Err: %7.3f; avErr: %7.3f; Speed: %7.3f; SyncOff: %7.3f", err, avErr, speed, syncOff - now))
		if speed > minFast or speed < maxSlow then
			mp.osd_message(string.format("Auto sync %1.2f", speed), interval)
		end
	end
end)

function toggleAutoSync()
	autoSync = not autoSync
	if autoSync then
		mp.osd_message("Auto sync on")
	else
		mp.osd_message("Auto sync off")
	end
	lastSpeed = 1.0
	mp.set_property("speed", lastSpeed)
end

-- Shift-A to toggle autosync on or off; initially off unless --script-opts=autosync-lag
mp.add_key_binding("A", "autoSync", toggleAutoSync)

-- press "(" to delay playback in 0.1 second increments
mp.add_key_binding("(", "lagMore", function ()
	offsetTarget = offsetTarget + 0.1
	if offsetTarget > 120.0 then
		offsetTarget = 120.0
	end
	mp.osd_message(string.format("Auto sync lag %1.1f", offsetTarget))
end)

-- press ")" to advance playback in 0.1 second increments
mp.add_key_binding(")", "lagLess", function ()
	offsetTarget = offsetTarget - 0.1
	if offsetTarget < 3.0 then
		offsetTarget = 3.0
	end
	mp.osd_message(string.format("Auto sync lag %1.1f", offsetTarget))
end)
