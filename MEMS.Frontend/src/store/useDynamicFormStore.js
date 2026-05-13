import { create } from 'zustand';
import { produce } from 'immer';

export const useDynamicFormStore = create((set, get) => ({
  schema: [], // Cấu trúc từ API
  formData: {}, // { "1.1": 100, "1.2": 200, "1": 300 }

  setSchema: (schema) => set({ schema }),

  updateValue: (fieldId, value) => set(produce(state => {
    state.formData[fieldId] = value;
    
    // Logic: Duyệt ngược từ dưới lên để cập nhật tổng cho cha
    const calculateParent = (id) => {
      const parts = id.split('.');
      if (parts.length <= 1) return; // Không còn cha (ví dụ: I, II)
      
      parts.pop();
      const parentId = parts.join('.');
      
      // Tìm tất cả con của parentId này trong schema
      const siblings = Object.keys(state.formData).filter(key => 
        key.startsWith(parentId + '.') && key.split('.').length === parts.length + 1
      );
      
      const total = siblings.reduce((sum, key) => sum + (Number(state.formData[key]) || 0), 0);
      state.formData[parentId] = total;
      
      // Tiếp tục đệ quy lên cấp cao hơn
      calculateParent(parentId);
    };

    calculateParent(fieldId);
  })),
}));