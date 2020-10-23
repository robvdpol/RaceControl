#!python
import sys
import os.path

basedir = os.path.abspath(os.path.dirname(__file__))
pkgdir = os.path.join(basedir, 'packages')
sys.path.insert(0, pkgdir)
os.environ['PYTHONPATH'] = os.pathsep.join([pkgdir, os.environ.get('PYTHONPATH', '')])

if __name__ == '__main__':
    from streamlink_cli.main import main
    main()
