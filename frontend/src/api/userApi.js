import axiosClient from './axiosClient';

const userApi = {
  // Lấy tất cả users
  getAllUsers: async () => {
    return await axiosClient.get('/User');
  },

  // Lấy user theo ID
  getUserById: async (id) => {
    return await axiosClient.get(`/User/${id}`);
  },

  // Lấy user theo username
  getUserByUsername: async (username) => {
    return await axiosClient.get(`/User/username/${username}`);
  },

  // Lấy user theo email
  getUserByEmail: async (email) => {
    return await axiosClient.get(`/User/email/${email}`);
  },

  // Lấy users active
  getActiveUsers: async () => {
    return await axiosClient.get('/User/active');
  },

  // Lấy users theo role
  getUsersByRole: async (role) => {
    return await axiosClient.get(`/User/role/${role}`);
  },

  // Tìm kiếm users
  searchUsers: async (searchTerm) => {
    return await axiosClient.get(`/User/search?searchTerm=${searchTerm}`);
  },

  // Tạo user mới
  createUser: async (userData) => {
    return await axiosClient.post('/User', userData);
  },

  // Cập nhật user
  updateUser: async (id, userData) => {
    return await axiosClient.put(`/User/${id}`, userData);
  },

  // Xóa user
  deleteUser: async (id) => {
    return await axiosClient.delete(`/User/${id}`);
  },

  // Đổi mật khẩu
  changePassword: async (id, passwordData) => {
    return await axiosClient.put(`/User/${id}/change-password`, passwordData);
  },

  // Reset mật khẩu (Admin only)
  resetPassword: async (id, newPassword) => {
    return await axiosClient.put(`/User/${id}/reset-password`, newPassword);
  },

  // Activate user
  activateUser: async (id) => {
    return await axiosClient.put(`/User/${id}/activate`);
  },

  // Deactivate user
  deactivateUser: async (id) => {
    return await axiosClient.put(`/User/${id}/deactivate`);
  },

  // Kiểm tra username có tồn tại không
  checkUsernameExists: async (username) => {
    return await axiosClient.get(`/User/check-username/${username}`);
  },

  // Kiểm tra email có tồn tại không
  checkEmailExists: async (email) => {
    return await axiosClient.get(`/User/check-email/${email}`);
  },

  // Lấy tổng số users
  getTotalUsersCount: async () => {
    return await axiosClient.get('/User/stats/total');
  },

  // Lấy số users active
  getActiveUsersCount: async () => {
    return await axiosClient.get('/User/stats/active');
  },

  // Kiểm tra có thể xóa user không
  canDeleteUser: async (id) => {
    return await axiosClient.get(`/User/${id}/can-delete`);
  },
};

export default userApi;