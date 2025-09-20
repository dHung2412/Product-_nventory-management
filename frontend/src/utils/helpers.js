// Format currency
export const formatCurrency = (amount) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
    }).format(amount);
  };
  
  // Format date
  export const formatDate = (date) => {
    return new Date(date).toLocaleDateString('vi-VN');
  };
  
  // Format datetime
  export const formatDateTime = (date) => {
    return new Date(date).toLocaleString('vi-VN');
  };
  
  // Capitalize first letter
  export const capitalizeFirst = (str) => {
    return str.charAt(0).toUpperCase() + str.slice(1);
  };
  
  // Get role display name
  export const getRoleDisplayName = (role) => {
    const roleNames = {
      Admin: 'Quản trị viên',
      Manager: 'Quản lý',
      Employee: 'Nhân viên',
      User: 'Người dùng',
    };
    return roleNames[role] || role;
  };
  
  // Get transaction type display name
  export const getTransactionTypeDisplayName = (type) => {
    const typeNames = {
      Import: 'Nhập kho',
      Export: 'Xuất kho',
      Adjustment: 'Điều chỉnh',
      Transfer: 'Chuyển kho',
    };
    return typeNames[type] || type;
  };
  
  // Validate email
  export const isValidEmail = (email) => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };
  
  // Validate phone number
  export const isValidPhone = (phone) => {
    const phoneRegex = /^[0-9]{10,11}$/;
    return phoneRegex.test(phone);
  };
  
  // Generate random ID
  export const generateId = () => {
    return Math.random().toString(36).substring(2) + Date.now().toString(36);
  };
  
  // Check if user has permission
  export const hasPermission = (userRole, requiredRoles) => {
    if (!requiredRoles || requiredRoles.length === 0) return true;
    return requiredRoles.includes(userRole);
  };
  
  // Debounce function
  export const debounce = (func, wait) => {
    let timeout;
    return function executedFunction(...args) {
      const later = () => {
        clearTimeout(timeout);
        func(...args);
      };
      clearTimeout(timeout);
      timeout = setTimeout(later, wait);
    };
  };
  
  // Handle API error
  export const handleApiError = (error) => {
    if (error.response) {
      // Server returned an error
      return error.response.data?.message || 'Có lỗi xảy ra';
    } else if (error.request) {
      // Network error
      return 'Không thể kết nối đến server';
    } else {
      // Other error
      return error.message || 'Có lỗi xảy ra';
    }
  };