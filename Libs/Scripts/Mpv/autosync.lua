secs = os.time()
acctime = secs
autosync = true

poff = 0 -- play offset
coff = 0 -- cache offset
coffmin = 1e50 -- bigger than maximum integer
cofftarget = 30 -- target max cache
synctime = 5 -- max seconds to resync
syncerr = 0.1 -- tolerated sync offset

-- work-around lack of sub-second system time
mp.add_periodic_timer(0.1, function()
    local newsecs = os.time()
    local cache
    if newsecs ~= secs then -- once a second we do this
		acctime = newsecs
		secs = newsecs
		pos = mp.get_property("time-pos")
		if pos ~= nil then
			cache = mp.get_property("demuxer-cache-duration")
			if cache ~= nil then
				poff = acctime - pos
				coff = poff - cache
				if (coff < coffmin) then
					coffmin = coff
				end
				local err = poff - coffmin - cofftarget
				local speed = 1
				if (err > syncerr) then
					speed = math.max(1.1, math.min(9.94, 1 + err / synctime))
				elseif (err < -syncerr) then
					speed = math.max(0.1, math.min(0.9, 1 + err / synctime))
				end
				if autosync then
					mp.set_property("speed", speed)
					if speed ~= 1 then
						mp.osd_message(string.format("Error: %6.1f; Speed: %3.1f", err, speed))
					end
				end
			end
		end
    else -- ten times a second we do this hack to get more time accuracy
		acctime = acctime + 0.1
    end
end)

mp.add_key_binding("A", "auto_sync", function ()
    autosync = not autosync
    if autosync then
		mp.osd_message("Auto sync on")
    else
		mp.set_property("speed", 1)
		mp.osd_message("Auto sync off")
    end
end)

mp.add_key_binding("D", "increase_offset", function ()
	cofftarget = math.min(cofftarget + 1, 60)
	mp.osd_message(string.format("Offset target %d seconds", cofftarget))
end)

mp.add_key_binding("S", "decrease_offset", function ()
	cofftarget = math.max(cofftarget - 1, 5)
    mp.osd_message(string.format("Offset target %d seconds", cofftarget))
end)