import { useEffect, useRef } from 'react';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';

// Fix default marker icon path (webpack/vite compat)
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
});

export default function MapView({ location, className = 'h-56' }) {
  const mapRef = useRef(null);
  const mapInstance = useRef(null);

  useEffect(() => {
    if (mapInstance.current) return;

    const map = L.map(mapRef.current, {
      zoomControl: false,
      attributionControl: false,
    }).setView([20, 78], 5);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
    }).addTo(map);

    // Try to geocode the location by using OpenStreetMap's Nominatim
    if (location) {
      fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(location)}&limit=1`)
        .then((res) => res.json())
        .then((data) => {
          if (data?.length > 0) {
            const { lat, lon } = data[0];
            map.setView([parseFloat(lat), parseFloat(lon)], 13);
            L.marker([parseFloat(lat), parseFloat(lon)])
              .addTo(map)
              .bindPopup(location);
          }
        })
        .catch(() => {});
    }

    mapInstance.current = map;

    return () => {
      map.remove();
      mapInstance.current = null;
    };
  }, [location]);

  return <div ref={mapRef} className={`${className} rounded-lg overflow-hidden border border-brand-divider`} />;
}
