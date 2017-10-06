--[[
    See script details on https://github.com/kevinlekiller/mpv_scripts
    
    Valid --script-opts are (they are all optional):
    autospeed-nircmd=false      true/false - Use nircmd to change the refresh rate of your monitor.
    autospeed-speed=false       true/false - Adjust speed of the video?.
    autospeed-nircmdc="nircmdc" String     - Path to nircmdc executable file. If not set, nircmdc will be searched in Windows PATH variable.
    autospeed-monitor=0         Number     - Which monitor (display) to set the refresh rate on.
    autospeed-dwidth=1920       Number     - Display width.
    autospeed-dheight=1080      Number     - Display height.
    autospeed-bdepth=32         Number     - Display bit depth.
    autospeed-rates="60"        String     - String of refresh rates your monitor supports and you want to use, separated by semicolons.
                                             Nircmd seems to prefer rounded numbers, 72 instead of 71.92 for example.
                                             Examples: autospeed-rates="60" | autospeed-rates="50;60;72"
                                             Note if you want a rate to be prefered over another, for example the video is 24hz
                                             and your rates are "100;120;144", but you want 144hz instead of 120hz for 24fps videos, change the
                                             order to autospeed-rates="100;144;120", then 144hz will take precendence over 120hz.
    autospeed-exitrate=60       Number     - Which refresh rate to set when exiting mpv. Set to 0 to ignore.
    autospeed-minspeed=0.9      Number     - Minimum allowable speed to play video at.
    autospeed-maxspeed=1.1      Number     - Maximum allowable speed to play video at.
    autospeed-osd=true          true/false - Enable OSD.
    autospeed-osdtime=10        Number     - How many seconds the OSD will be shown.
    autospeed-osdkey=y                     - Key to press to show the OSD.
    autospeed-estfps=false      true/false - Calculate/change speed if a video has a variable fps at the cost of higher CPU usage (most videos have a fixed fps).
    autospeed-spause            true/false - Pause video while switching display modes.
                                Number     - If you set this a number, it will pause for that amount of seconds.
    autospeed-method="always"   String     - Set how often to check if framerate conforms to settings
					     "always": (original behaviour) contstantly checks during playback
					     "start": only does initial change at start

    Example: mpv file.mkv --script-opts=autospeed-nircmd=true,autospeed-minspeed=0.8
--]]
--[[
    Copyright (C) 2016-2017  kevinlekiller

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
    https://www.gnu.org/licenses/gpl-2.0.html
--]]
local _global = {
    osd_start = mp.get_property_osd("osd-ass-cc/0"),
    osd_end = mp.get_property_osd("osd-ass-cc/1"),
    utils = require 'mp.utils',
    rateCache = {},
    lastDrr = 0,
    initialRefreshFound = false,
    speedCache = {},
    next = next,
}

function round(number)
    return math.floor(number + 0.5)
end

function osdEcho()
    if (_global.options["osd"] ~= true) then
        return
    end
    setOSD()
    if (_global.temp["output"] ~= nil) then
        mp.osd_message(_global.temp["output"], _global.options["osdtime"])
    end
end

function getOptions()
    _global.options = {
        ["nircmd"]    = false,
        ["speed"]     = false,
        ["nircmdc"]   = "nircmdc",
        ["monitor"]   = 0,
        ["dwidth"]    = 1920,
        ["dheight"]   = 1080,
        ["bdepth"]    = 32,
        ["rates"]     = "",
        ["exitrate"]  = 0,
        ["minspeed"]  = 0.9,
        ["maxspeed"]  = 1.1,
        ["osd"]       = false,
        ["osdtime"]   = 10,
        ["osdkey"]    = "y",
        ["estfps"]    = false,
        ["spause"]    = 0,
	["method"]   = "once",
    }
    for key, value in pairs(_global.options) do
        local opt = mp.get_opt("autospeed-" .. key)
        if (opt ~= nil) then
            if ((key == "nircmd" or key == "speed" or key == "osd" or key == "estfps") and opt == "true") then
                _global.options[key] = true
            elseif (key == "minspeed" or key == "maxspeed" or key == "osdtime" or key == "monitor" or key == "dwidth" or key == "dheight" or key == "bdepth" or key == "exitrate" or key == "spause") then
                local test = tonumber(opt)
                if (test ~= nil) then
                    _global.options[key] = test
                end
            else
                _global.options[key] = opt
            end
        end
    end
end
getOptions()

function main(name, fps)
    if ((_global.initialRefreshFound == false) or (_global.options["method"] == "always")) then
	    if (fps == nil) then
		return
	    end
	    _global.temp["fps"] = fps
	    findRefreshRate()
	    determineSpeed()
	    if (_global.options["speed"] == true and _global.temp["speed"] >= _global.options["minspeed"] and _global.temp["speed"] <= _global.options["maxspeed"]) then
		mp.set_property_number("speed", _global.temp["speed"])
	    else
		_global.temp["speed"] = _global.confSpeed
	    end
	    _global.initialRefreshFound = true
    end
end

function setOSD()
    _global.temp["output"] = (_global.osd_start ..
        "{\\b1}Original monitor refresh rate{\\b0}\\h\\h" .. _global.temp["start_drr"] .. "Hz\\N" ..
        "{\\b1}Current  monitor refresh rate{\\b0}\\h\\h" .. _global.temp["drr"] .. "Hz\\N" ..
        "{\\b1}Original video fps{\\b0}\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h" .. _global.temp["fps"] .. "fps\\N" ..
        "{\\b1}Current  video fps{\\b0}\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h\\h" .. (_global.temp["fps"] * _global.temp["speed"]) .. "fps\\N" ..
        "{\\b1}Original mpv speed setting{\\b0}\\h\\h\\h\\h\\h\\h" .. _global.confSpeed .. "x\\N" ..
        "{\\b1}Current  mpv speed setting{\\b0}\\h\\h\\h\\h\\h\\h" .. _global.temp["speed"] .. "x" ..
        _global.osd_end
    )
end

function determineSpeed()
    local id = _global.temp["drr"] .. _global.temp["fps"]
    if (_global.speedCache[id] ~= nil) then
        _global.temp["speed"] = _global.speedCache[id]
        return
    end
    if (_global.temp["drr"] > _global.temp["fps"]) then
        local difference = (_global.temp["drr"] / _global.temp["fps"])
        if (difference >= 2) then
            -- fps = 24fps, drr = 60hz
            -- difference = 60hz/24fps = 3 rounded
            -- 24fps * 3 = 72fps
            -- 60hz / 72fps = 0.833333333333 speed
            -- 72fps * 0.833333333333 = 60fps
            _global.temp["speed"] = (_global.temp["drr"] / (_global.temp["fps"] * round(difference)))
        else
            -- fps = 50fps, drr = 60hz
            -- 60hz / 50fps = 1.2 speed
            -- 50fps * 1.2 speed = 60fps
            
            -- fps = 59.94fps, drr = 60hz
            -- 60hz / 59.94fps  = 1.001001001001001 speed
            -- 59.94fps * 1.001001001001001 = 60fps
            _global.temp["speed"] = difference
        end
    elseif (_global.temp["drr"] < _global.temp["fps"]) then
        local difference = (_global.temp["fps"] / _global.temp["drr"])
        if (difference >= 2) then
            -- fps = 120fps, drr = 25hz
            -- difference = 120fps/25hz = 5 rounded
            -- 120fps/5 = 24fps ; 25hz / 24fps = 1.04166666667 speed
            -- 24fps * 1.04166666667 speed = 25fps
            _global.temp["speed"] = (_global.temp["drr"] / (_global.temp["fps"] / round(difference)))
        else
            -- fps = 60fps, drr = 50hz
            -- difference = 50hz / 60fps = 0.833333333333 speed
            -- 60fps * 0.833333333333 speed = 50fps
            
            -- fps = 60fps, drr = 59.94hz
            -- difference = 59.94hz / 60fps = 0.999 speed
            -- 60fps * 0.999 speed = 59.94fps
            _global.temp["speed"] = (_global.temp["drr"] / _global.temp["fps"])
        end
    elseif (_global.temp["drr"] == _global.temp["fps"]) then
        _global.temp["speed"] = 1
    end
    _global.speedCache[id] = _global.temp["speed"]
end

function findRefreshRate()
    -- This is to prevent a system call if the screen refresh / video fps has not changed.
    if (_global.temp["drr"] == _global.lastDrr) then
        return
    elseif (_global.rateCache[_global.temp["drr"]] ~= nil) then
        setRate(_global.rateCache[_global.temp["drr"]])
        return
    end
    if (_global.options["nircmd"] ~= true or _global.options["rates"] == "") then
        return
    end
    local round_fps = round(_global.temp["fps"])
    if (_global.temp["maxrate"] == nil) then
        _global.temp["maxrate"] = 0
        for rate in string.gmatch(_global.options["rates"], "[%w.]+") do
            rate = tonumber(rate)
            if (rate > _global.temp["maxrate"]) then
                _global.temp["maxrate"] = rate
            end
        end
        if (_global.temp["maxrate"] == 0) then
            _global.options["rates"] = ""
            return
        end
    end
    local iterator = 1
    if (_global.temp["maxrate"] > round_fps) then
        iterator = round(_global.temp["maxrate"] / round_fps)
    elseif (_global.temp["maxrate"] < round_fps) then
        iterator = round(round_fps / _global.temp["maxrate"])
    else
        setRate(_global.temp["maxrate"])
        return
    end
    local smallest = 0
    local foundRate = false
    for rate in string.gmatch(_global.options["rates"], "[%w.]+") do
        rate = tonumber(rate)
        local min = (rate * _global.options["minspeed"])
        local max = (rate * _global.options["maxspeed"])
        for multiplier = 1, iterator do
            local multiplied_fps = (multiplier * round_fps)
            if (multiplied_fps >= min and multiplied_fps <= max) then
                if (multiplied_fps < rate) then
                    local difference = (rate - multiplied_fps)
                    if (smallest == 0 or difference < smallest) then
                        smallest = difference
                        foundRate = rate
                    end
                elseif (multiplied_fps > rate) then
                    local difference = (multiplied_fps - rate)
                    if (smallest == 0 or difference < smallest) then
                        smallest = difference
                        foundRate = rate
                    end
                else
                    setRate(rate)
                    return
                end
            end
        end
    end
    if (foundRate ~= false) then
        setRate(foundRate)
    end
end

function setRate(rate)
    local paused = mp.get_property("pause")
    if (_global.options["spause"] > 0 and paused ~= "yes") then
	mp.set_property("pause", "yes")
    end
    _global.utils.subprocess({
	["cancellable"] = false,
	["args"] = {
	    [1] = _global.options["nircmdc"],
	    [2] = "setdisplay",
	    [3] = "monitor:" .. _global.options["monitor"],
	    [4] = _global.options["dwidth"],
	    [5] = _global.options["dheight"],
	    [6] = _global.options["bdepth"],
	    [7] = rate
	}
    })
    if (_global.options["spause"] > 0 and paused ~= "yes") then
		--os.execute("ping -n " .. _global.options["spause"] .. " localhost > NUL")
		_global.utils.subprocess({
			["cancellable"] = false,
			["args"] = {
				[1] = "ping",
				[2] = "-n",
				[3] = _global.options["spause"],
				[4] = "localhost",
				[5] = ">",
				[6] = "NUL"
			}
		})
	mp.set_property("pause", "no")
    end
	--os.execute("ping -n 2 localhost > NUL")
	_global.utils.subprocess({
			["cancellable"] = false,
			["args"] = {
				[1] = "ping",
				[2] = "-n",
				[3] = "2",
				[4] = "localhost",
				[5] = ">",
				[6] = "NUL"
			}
		})
    _global.temp["drr"] = mp.get_property_native("display-fps")
    _global.rateCache[_global.temp["drr"]] = rate
    _global.lastDrr = _global.temp["drr"]
end

function start()
    mp.unobserve_property(start)
    _global.temp = {}
    _global.temp["start_drr"] = mp.get_property_native("display-fps")
    _global.options["monitor"] = mp.get_property_native("display-names")
    _global.options["exitrate"] = _global.temp["start_drr"]
	
    if not (_global.temp["start_drr"]) then
        return
    end
    _global.temp["drr"] = _global.temp["start_drr"]
    if not (_global.confSpeed) then
        _global.confSpeed = mp.get_property_native("speed")
    end
    
	local test = mp.get_property("container-fps")
    _global.options["rates"] = test
	
    if (test == nil or test == "nil property unavailable") then
        if (_global.options["estfps"] ~= true) then
            return
        end
        test = mp.get_property("estimated-vf-fps")
        if (test == nil or test == "nil property unavailable") then
            return
        end
	_global.options["rates"] = test
        mp.observe_property("estimated-vf-fps", "number", main)
    else
        mp.observe_property("container-fps", "number", main)
    end
    mp.add_key_binding(_global.options["osdkey"], mp.get_script_name(), osdEcho, {repeatable=true})
    if (_global.options["nircmd"] == true and _global.options["exitrate"] > 0) then
        function revertDrr()
            _global.utils.subprocess({
                ["cancellable"] = false,
                ["args"] = {
                    [1] = _global.options["nircmdc"],
                    [2] = "setdisplay",
                    [3] = "monitor:" .. _global.options["monitor"],
                    [4] = _global.options["dwidth"],
                    [5] = _global.options["dheight"],
                    [6] = _global.options["bdepth"],
                    [7] = _global.options["exitrate"]
                }
            })
        end
        mp.register_event("shutdown", revertDrr)
    end
end

-- Wait until we get a video fps.
function check()
    mp.observe_property("estimated-vf-fps", "string", start)
end

mp.register_event("file-loaded", check)
