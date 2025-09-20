import axiosClient from './axiosClient';

const authApi = {
  // Đăng nhập
  login: async (loginData) => {
    return await axiosClient.post('/Auth/login', loginData);
  },

  // Đăng ký
  register: async (registerData) => {
    return await axiosClient.post('/Auth/register', registerData);
  },

  // Refresh token
  refreshToken: async (refreshToken) => {
    return await axiosClient.post('/Auth/refresh', { refreshToken });
  },

  // Đăng xuất
  logout: async (refreshToken) => {
    return await axiosClient.post('/Auth/logout', { refreshToken });
  },

  // Validate token
  validateToken: async () => {
    return await axiosClient.get('/Auth/validate');
  },

  // Lấy thông tin user hiện tại
  getCurrentUser: async () => {
    return await axiosClient.get('/Auth/me');
  },
};

export default authApi;