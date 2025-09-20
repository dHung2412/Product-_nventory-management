import axiosClient from './axiosClient';

const warehouseApi = {
  // Lấy tất cả kho
  getAll: async () => {
    return await axiosClient.get('/Warehouse');
  },

  // Lấy kho theo ID
  getById: async (id) => {
    return await axiosClient.get(`/Warehouse/${id}`);
  },

  // Lấy stock trong kho
  getWarehouseStock: async (id) => {
    return await axiosClient.get(`/Warehouse/${id}/stock`);
  },

  // Tạo kho mới
  create: async (warehouseData) => {
    return await axiosClient.post('/Warehouse', warehouseData);
  },

  // Cập nhật kho
  update: async (id, warehouseData) => {
    return await axiosClient.put(`/Warehouse/${id}`, warehouseData);
  },

  // Xóa kho
  delete: async (id) => {
    return await axiosClient.delete(`/Warehouse/${id}`);
  },

  // Lấy thống kê kho
  getStatistics: async () => {
    return await axiosClient.get('/Warehouse/statistics');
  },

  // Kiểm tra có thể xóa kho không
  canDelete: async (id) => {
    return await axiosClient.get(`/Warehouse/${id}/can-delete`);
  },

  // Kiểm tra kho có tồn tại không
  exists: async (id) => {
    try {
      await axiosClient.head(`/Warehouse/${id}`);
      return true;
    } catch {
      return false;
    }
  },
};

export default warehouseApi;