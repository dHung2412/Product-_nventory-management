// frontend/src/api/productApi.js
import axiosClient from './axiosClient';

export const productApi = {
  // Lấy tất cả sản phẩm
  getAll: async () => {
    const response = await axiosClient.get('/Product');
    return response;
  },

  // Lấy sản phẩm theo ID
  getById: async (id) => {
    const response = await axiosClient.get(`/Product/${id}`);
    return response;
  },

  // Tạo sản phẩm mới
  create: async (productData) => {
    const response = await axiosClient.post('/Product', productData);
    return response;
  },

  // Cập nhật sản phẩm
  update: async (id, productData) => {
    const response = await axiosClient.put(`/Product/${id}`, {
      ...productData,
      id: id
    });
    return response;
  },

  // Xóa sản phẩm
  delete: async (id) => {
    await axiosClient.delete(`/Product/${id}`);
  },

  // Tìm kiếm sản phẩm
  search: async (searchTerm) => {
    const response = await axiosClient.get(`/Product/search?term=${encodeURIComponent(searchTerm)}`);
    return response;
  },

  // Lấy sản phẩm theo danh mục
  getByCategory: async (category) => {
    const response = await axiosClient.get(`/Product/category/${encodeURIComponent(category)}`);
    return response;
  },

  // Lấy danh sách danh mục
  getCategories: async () => {
    const response = await axiosClient.get('/Product/categories');
    return response;
  },

  // Lấy sản phẩm sắp hết hàng
  getLowStock: async (threshold = 20) => {
    const response = await axiosClient.get(`/Product/low-stock?threshold=${threshold}`);
    return response;
  },

  // Lấy thống kê sản phẩm
  getStatistics: async () => {
    const response = await axiosClient.get('/Product/statistics');
    return response;
  }
};

export default productApi;