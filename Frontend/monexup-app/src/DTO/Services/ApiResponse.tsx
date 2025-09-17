interface ApiResponse<T> {
  httpStatus: string;
  messageError: string;
  success: boolean;
  data: T;
}

export default ApiResponse; 