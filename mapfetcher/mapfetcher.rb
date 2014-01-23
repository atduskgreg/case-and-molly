lat = 42.3603
lng = -71.08726
zoom = 17

def get_tile_number(lat_deg, lng_deg, zoom)
  lat_rad = lat_deg/180 * Math::PI
  n = 2.0 ** zoom
  x = ((lng_deg + 180.0) / 360.0 * n).to_i
  y = ((1.0 - Math::log(Math::tan(lat_rad) + (1 / Math::cos(lat_rad))) / Math::PI) / 2.0 * n).to_i
  
  {:x => x, :y =>y}
end

def get_lat_lng_for_number(xtile, ytile, zoom)
  n = 2.0 ** zoom
  lon_deg = xtile / n * 360.0 - 180.0
  lat_rad = Math::atan(Math::sinh(Math::PI * (1 - 2 * ytile / n)))
  lat_deg = 180.0 * (lat_rad / Math::PI)
  {:lat_deg => lat_deg, :lng_deg => lon_deg}
end

def get_bounding_box_for_lat_lng(lat_deg, lng_deg, width, height)

end



tile = get_tile_number(lat, lng, zoom)

top_left = get_lat_lng_for_number(tile[:x], tile[:y], zoom)
center = get_lat_lng_for_number(tile[:x] + 0.5, tile[:y] + 0.5, zoom)
bottom_right = get_lat_lng_for_number(tile[:x] + 1, tile[:y] + 1, zoom)

url = "http://a.tile.openstreetmap.org/#{zoom}/#{tile[:x]}/#{tile[:y]}.png"
puts url

puts "original: #{lat},#{lng}"
puts "bounds: center: #{center[:lat_deg]}, #{center[:lng_deg]}\ntop:#{top_left[:lat_deg]},#{top_left[:lng_deg]}\nbottom:#{bottom_right[:lat_deg]},#{bottom_right[:lng_deg]}"

`wget #{url}`