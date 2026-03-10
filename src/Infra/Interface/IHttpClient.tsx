import ApiResponse from "../../DTO/Services/ApiResponse";
//import { History } from 'history';

interface IHttpClient {
  init: (baseUrl:string) => void;
  setLogoff: (logoffCallback: () => void) => void;
  doPost: <T>(path:string, parameters:any) => Promise<ApiResponse<T>>;
  doPostAuth: <T>(path:string, parameters:any, tokenAuth: string) => Promise<ApiResponse<T>>;
  doPutAuth: <T>(path:string, parameters:any, tokenAuth: string) => Promise<ApiResponse<T>>;
  doGetAuth: <T>(path:string, tokenAuth:string) => Promise<ApiResponse<T>>;
  doGet: <T>(path:string, parameters:any) => Promise<ApiResponse<T>>;
  doPostFormData: <T>(path:string, parameters:FormData) => Promise<ApiResponse<T>>;
  doPostFormDataAuth: <T>(path:string, parameters:FormData, tokenAuth:string) => Promise<ApiResponse<T>>;
}

export default IHttpClient;