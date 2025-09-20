import axios from 'axios';
import { getToken, removeToken } from '../utils/storage';

const axiosClient = axios.create({
  baseURL: process.env.REACT_APP_API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor để thêm token vào header
axiosClient.interceptors.request.use(
  (config) => {
    const token = getToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor để xử lý lỗi chung
axiosClient.interceptors.response.use(
  (response) => {
    return response.data;
  },
  (error) => {
    if (error.response) {
      // Server trả về lỗi
      const { status, data } = error.response;
      
      if (status === 401) {
        // Token hết hạn hoặc không hợp lệ
        removeToken();
        window.location.href = '/login';
      }
      
      // Trả về message từ server hoặc message mặc định
      const errorMessage = data?.message || data?.Message || 'An error occurred';
      return Promise.reject(new Error(errorMessage));
    } else if (error.request) {
      // Lỗi network
      return Promise.reject(new Error('Network error. Please check your connection.'));
    } else {
      // Lỗi khác
      return Promise.reject(new Error('Something went wrong.'));
    }
  }
);

export default axiosClient;