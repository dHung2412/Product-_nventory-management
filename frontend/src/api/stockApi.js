import axiosClient from './axiosClient';

const stockApi = {
  // === Stock Items ===
  // Lấy tất cả stock items
  getAllStockItems: async () => {
    return await axiosClient.get('/Stock/items');
  },

  // Lấy stock item theo ID
  getStockItemById: async (id) => {
    return await axiosClient.get(`/Stock/items/${id}`);
  },

  // Lấy stock items theo product ID
  getStockItemsByProduct: async (productId) => {
    return await axiosClient.get(`/Stock/items/product/${productId}`);
  },

  // Lấy stock items theo warehouse ID
  getStockItemsByWarehouse: async (warehouseId) => {
    return await axiosClient.get(`/Stock/items/warehouse/${warehouseId}`);
  },

  // Tạo stock item mới
  createStockItem: async (stockItemData) => {
    return await axiosClient.post('/Stock/items', stockItemData);
  },

  // Cập nhật số lượng stock
  updateStockQuantity: async (id, quantityData) => {
    return await axiosClient.put(`/Stock/items/${id}/quantity`, quantityData);
  },

  // Thêm stock
  addStock: async (id, addStockData) => {
    return await axiosClient.post(`/Stock/items/${id}/add`, addStockData);
  },

  // Trừ stock
  removeStock: async (id, removeStockData) => {
    return await axiosClient.post(`/Stock/items/${id}/remove`, removeStockData);
  },

  // Xóa stock item
  deleteStockItem: async (id) => {
    return await axiosClient.delete(`/Stock/items/${id}`);
  },

  // === Stock Transactions ===
  // Lấy tất cả transactions
  getAllTransactions: async () => {
    return await axiosClient.get('/Stock/transactions');
  },

  // Lấy transaction theo ID
  getTransactionById: async (id) => {
    return await axiosClient.get(`/Stock/transactions/${id}`);
  },

  // Lấy transactions theo product ID
  getTransactionsByProduct: async (productId) => {
    return await axiosClient.get(`/Stock/transactions/product/${productId}`);
  },

  // Lấy transactions theo warehouse ID
  getTransactionsByWarehouse: async (warehouseId) => {
    return await axiosClient.get(`/Stock/transactions/warehouse/${warehouseId}`);
  },

  // Lấy transactions theo user ID
  getTransactionsByUser: async (userId) => {
    return await axiosClient.get(`/Stock/transactions/user/${userId}`);
  },

  // Lấy transactions theo type
  getTransactionsByType: async (type) => {
    return await axiosClient.get(`/Stock/transactions/type/${type}`);
  },

  // Lấy transactions theo khoảng thời gian
  getTransactionsByDateRange: async (fromDate, toDate) => {
    return await axiosClient.get(`/Stock/transactions/date-range?fromDate=${fromDate}&toDate=${toDate}`);
  },

  // === Business Operations ===
  // Nhập kho
  importStock: async (importData) => {
    return await axiosClient.post('/Stock/import', importData);
  },

  // Xuất kho
  exportStock: async (exportData) => {
    return await axiosClient.post('/Stock/export', exportData);
  },

  // Điều chỉnh kho
  adjustStock: async (adjustData) => {
    return await axiosClient.post('/Stock/adjust', adjustData);
  },

  // === Analytics and Reports ===
  // Lấy danh sách hàng sắp hết
  getLowStockItems: async () => {
    return await axiosClient.get('/Stock/low-stock');
  },

  // Lấy danh sách hàng dư thừa
  getOverStockItems: async () => {
    return await axiosClient.get('/Stock/over-stock');
  },

  // Lấy tóm tắt stock theo warehouse
  getStockSummaryByWarehouse: async (warehouseId) => {
    return await axiosClient.get(`/Stock/summary/warehouse/${warehouseId}`);
  },

  // Kiểm tra stock có sẵn
  getAvailableStock: async (productId, warehouseId) => {
    return await axiosClient.get(`/Stock/available?productId=${productId}&warehouseId=${warehouseId}`);
  },

  // Lấy thống kê stock
  getStatistics: async () => {
    return await axiosClient.get('/Stock/statistics');
  },
};

export default stockApi;