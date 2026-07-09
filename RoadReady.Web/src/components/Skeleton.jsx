export default function Skeleton({ className = '', height = 'h-4', width = 'w-full' }) {
  return (
    <div
      className={`bg-gray-100 rounded-xl animate-pulse ${height} ${width} ${className}`}
    />
  );
}

export function CardSkeleton({ lines = 3 }) {
  return (
    <div className="card p-5 space-y-3">
      <Skeleton height="h-40" />
      {Array.from({ length: lines }).map((_, i) => (
        <Skeleton key={i} height="h-4" width={i === 0 ? 'w-3/4' : i === 1 ? 'w-1/2' : 'w-full'} />
      ))}
    </div>
  );
}

export function TableSkeleton({ rows = 5 }) {
  return (
    <div className="card p-4 space-y-3">
      <Skeleton height="h-8" />
      {Array.from({ length: rows }).map((_, i) => (
        <Skeleton key={i} height="h-12" />
      ))}
    </div>
  );
}
