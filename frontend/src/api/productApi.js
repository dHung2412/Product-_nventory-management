import axiosClient from './axiosClient';

const productApi = {
  // Lấy tất cả sản phẩm
  getAll: async () => {
    return await axiosClient.get('/Product');
  },

  // Lấy sản phẩm theo ID
  getById: async (id) => {
    return await axiosClient.get(`/Product/${id}`);
  },

  // Lấy sản phẩm theo category
  getByCategory: async (category) => {
    return await axiosClient.get(`/Product/category/${category}`);
  },

  // Tìm kiếm sản phẩm
  search: async (searchTerm) => {
    return await axiosClient.get(`/Product/search?searchTerm=${searchTerm}`);
  },

  // Lấy danh sách categories
  getCategories: async () => {
    return await axiosClient.get('/Product/categories');
  },

  // Tạo sản phẩm mới
  create: async (productData) => {
    return await axiosClient.post('/Product', productData);
  },

  // Cập nhật sản phẩm
  update: async (id, productData) => {
    return await axiosClient.put(`/Product/${id}`, productData);
  },

  // Xóa sản phẩm
  delete: async (id) => {
    return await axiosClient.delete(`/Product/${id}`);
  },

  // Lấy thống kê sản phẩm
  getStatistics: async () => {
    return await axiosClient.get('/Product/statistics');
  },

  // Kiểm tra sản phẩm có tồn tại không
  exists: async (id) => {
    try {
      await axiosClient.head(`/Product/${id}`);
      return true;
    } catch {
      return false;
    }
  },
};

export default productApi;