// Shared JSDoc types
/** @typedef {'Customer'|'Admin'|'RentalAgent'} UserRole */

/**
 * @typedef {Object} UserDto
 * @property {string} id
 * @property {string} firstName
 * @property {string} lastName
 * @property {string} email
 * @property {string} phoneNumber
 * @property {string|null} profileImageUrl
 * @property {UserRole} role
 * @property {boolean} isActive
 * @property {string} createdAt
 */

/**
 * @typedef {Object} AuthTokens
 * @property {string} accessToken
 * @property {string} refreshToken
 * @property {UserDto} user
 */

/**
 * @typedef {Object} Brand
 * @property {number} id
 * @property {string} name
 * @property {string} [logoUrl]
 * @property {string} [country]
 */

/**
 * @typedef {'Available'|'Rented'|'UnderMaintenance'|'Inactive'} CarStatus
 */

/**
 * @typedef {Object} Review
 * @property {string} id
 * @property {number} carId
 * @property {string} userId
 * @property {string} userName
 * @property {string|null} [userProfileImage]
 * @property {number} rating
 * @property {string} comment
 * @property {string} createdAt
 */

/**
 * @typedef {Object} Car
 * @property {number} id
 * @property {string} make
 * @property {string} model
 * @property {number} year
 * @property {string} color
 * @property {string} licensePlate
 * @property {string} location
 * @property {number} pricePerDay
 * @property {string} transmission
 * @property {string} fuelType
 * @property {number} seatingCapacity
 * @property {string} description
 * @property {string[]} imageUrls
 * @property {CarStatus} status
 * @property {number} brandId
 * @property {string} brandName
 * @property {number} averageRating
 * @property {number} reviewCount
 */

/**
 * @typedef {Object} PagedResponse
 * @property {boolean} success
 * @property {string} [message]
 * @property {T[]} data
 * @property {number} page
 * @property {number} pageSize
 * @property {number} totalCount
 * @property {number} totalPages
 * @property {boolean} hasNextPage
 * @property {boolean} hasPreviousPage
 * @template T
 */

/**
 * @typedef {'PendingPayment'|'Confirmed'|'Cancelled'|'Modified'|'Active'|'Completed'} BookingStatus
 */

/**
 * @typedef {Object} Booking
 * @property {number} id
 * @property {string} userId
 * @property {number} carId
 * @property {string} carMake
 * @property {string} carModel
 * @property {string} carImageUrl
 * @property {string} pickupDate
 * @property {string} dropoffDate
 * @property {string} pickupLocation
 * @property {boolean} includesCarSeat
 * @property {string|null} [appliedPromoCode]
 * @property {number} subtotal
 * @property {number} discountAmount
 * @property {number} totalAmount
 * @property {BookingStatus} status
 * @property {string} paymentUrl
 * @property {string} paymentStatus
 * @property {string|null} [receiptUrl]
 * @property {string} createdAt
 */

/**
 * @typedef {Object} Payment
 * @property {number} id
 * @property {number} bookingId
 * @property {string} userId
 * @property {number} amount
 * @property {'InitialCharge'|'Refund'} type
 * @property {'Pending'|'Succeeded'|'Failed'} status
 * @property {string|null} [paymentUrl]
 * @property {string} createdAt
 */

/**
 * @typedef {Object} PromoCode
 * @property {number} id
 * @property {string} code
 * @property {string} description
 * @property {'Percentage'|'FlatAmount'} discountType
 * @property {number} discountValue
 * @property {number|null} [minBookingAmount]
 * @property {string} validFrom
 * @property {string} validUntil
 * @property {number|null} [maxUses]
 * @property {number} currentUses
 * @property {boolean} isActive
 */

/**
 * @typedef {Object} AdminAnalytics
 * @property {number} totalReservations
 * @property {number} activeBookings
 * @property {number} cancelledBookings
 * @property {number} totalRevenue
 * @property {number} totalRefunded
 * @property {number} netRevenue
 */

export {};
