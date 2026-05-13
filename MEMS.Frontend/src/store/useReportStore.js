import { create } from 'zustand';
import { produce } from 'immer';

// Thuật toán đệ quy DFS: Cập nhật Lá và Rollup lại Tổng các Cha
const updateTreeAndRollup = (nodes, nodeId, newValue) => {
    let hasChanged = false;
    let newTotal = 0;

    for (let node of nodes) {
        // Cập nhật nốt lá
        if (node.id === nodeId && node.isLeaf) {
            node.totalValue = newValue;
            hasChanged = true;
        }

        // Đệ quy chui xuống các nốt con
        if (node.children && node.children.length > 0) {
            const childChanged = updateTreeAndRollup(node.children, nodeId, newValue);
            if (childChanged) {
                hasChanged = true;
            }
            
            // Tính lại tổng cho nốt cha bằng cách cộng dồn tất cả giá trị con
            if (!node.isLeaf) {
                node.totalValue = node.children.reduce((sum, child) => sum + (child.totalValue || 0), 0);
            }
        }

        newTotal += node.totalValue || 0;
    }

    return hasChanged;
};

const useReportStore = create((set) => ({
    categoryTree: [],
    isLoading: false,
    
    // ✅ NEW: Track danh sách reports đã upload
    reports: [],
    
    // ✅ NEW: Track report hiện tại được chọn
    selectedReportId: null,

    setCategoryTree: (treeData) => set({ categoryTree: treeData }),

    // ✅ NEW: Lưu danh sách reports từ API
    setReports: (reportsList) => set({ reports: reportsList }),

    // ✅ NEW: Thêm report mới vào danh sách (sau khi upload)
    addReport: (report) => set((state) => ({
        reports: [...state.reports, report],
        selectedReportId: report.id  // Auto-select report vừa upload
    })),

    // ✅ NEW: Chọn report để export
    setSelectedReportId: (reportId) => set({ selectedReportId: reportId }),

    // Zero-Lag Update: Hàm này chạy rất nhẹ vì Immutable tree được quản lý bởi Immer
    updateLeafValue: (nodeId, newValue) => 
        set(produce((draft) => {
            // thao tác trực tiếp trên draft mà không cần clone sâu (Immer tự lo việc clone)
            updateTreeAndRollup(draft.categoryTree, nodeId, newValue);
        })),

    setLoading: (status) => set({ isLoading: status })
}));

export default useReportStore;