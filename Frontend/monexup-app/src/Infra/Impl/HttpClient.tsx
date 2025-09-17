import IHttpClient from "../Interface/IHttpClient";
import axios, { AxiosInstance } from 'axios';
import ApiResponse from "../../DTO/Services/ApiResponse";


let logoff : () => void; 

function getCatchValue<T>(error: any, path: string) : ApiResponse<T> {
  let ret : ApiResponse<T>;
  if (error.response) {
    console.error("ops! ocorreu um erro na solicitação do endpoint: " + path  + "\nHttp Status:" + error.response.status + "\n Descrição: " + JSON.stringify(error.response.data));
    if(error.response.status.toString() === "401") {
      logoff();
      window.location.href = "/login";
    }
    ret = {
      httpStatus: error.response.status.toString(),
      success: false,
      messageError: error.response.data,
      ...ret
    }
  } else if (error.request) {
    console.error("Não foi possível receber nenhuma resposta na solicitação do endpoint: " + path  + "\n Descrição: " + error.request);
    ret = {
      httpStatus: "400",
      success: false,
      messageError: error.request,
      ...ret
    }
  } else {
    console.error("Erro desconhecido na solicitação do endpoint: " + path  + "\n Descrição: " + error.message);
    ret = {
      httpStatus: "400",
      success: false,
      messageError: error.message,
      ...ret
    }
  }
  return ret;
}

const HttpClient  = () : IHttpClient => {
  let axiosIntance : AxiosInstance;
  return {
    init: (baseUrl: string) => {
      axiosIntance = axios.create({
        baseURL: baseUrl,
      });
    },
    setLogoff: (logoffCallback: () => void) => {
      logoff = logoffCallback
    },
    doPost: async function <T>(path: string, parameters: any): Promise<ApiResponse<T>> {
      let ret: ApiResponse<T>;
      if(process.env.REACT_APP_PRODUCTION == "0")
        console.info("Requisição realizada: \n\tURL:" + path + "\n\tParâmetros: " + JSON.stringify(parameters));
      await axiosIntance.post(path, parameters)
        .then((response) => {
          ret = {
            data: response.data,
            httpStatus: response.status.toString(),
            success: true,
            ...ret
          };
        })
        .catch((error) => {
          console.log(`Erro no HTTPClient ${JSON.stringify(error)}`);
          ret = getCatchValue<T>(error, path);
        });
      return ret;
    },
    doPostAuth: async function <T>(path: string, parameters: any, tokenAuth: string): Promise<ApiResponse<T>> {
      let ret: ApiResponse<T>;
      if(process.env.REACT_APP_PRODUCTION == "0")
        console.info("Requisição com token realizada: \n\tURL:" + path + "\n\tParâmetros: " + JSON.stringify(parameters) + "\n\tToken: " + tokenAuth);
      await axiosIntance.post(path, parameters, {
        headers: {
          "Content-Type": "application/json",
          "Authorization": "Basic " + tokenAuth
        }
      })
        .then((response) => {
          ret = {
            data: response.data,
            httpStatus: response.status.toString(),
            success: true,
            ...ret
          };
        })
        .catch((error) => {
          ret = getCatchValue<T>(error, path);
        });
      return ret;
    },
    doGetAuth: async function <T>(path: string, tokenAuth: string): Promise<ApiResponse<T>> {
      let ret: ApiResponse<T>;
      if(process.env.REACT_APP_PRODUCTION == "0")
        console.info("Requisição com token realizada: \n\tURL:" + path + "\n\tToken: " + tokenAuth);
      await axiosIntance.get(path, {
        headers: {
          "Content-Type": "application/json",
          "Authorization": "Basic " + tokenAuth
        }
      })
        .then((response) => {
          ret = {
            data: response.data,
            httpStatus: response.status.toString(),
            success: true,
            ...ret
          };
        })
        .catch((error) => {        
          ret = getCatchValue<T>(error, path);
        });
      return ret;
    },
    doGet: async function <T>(path: string, parameters: any): Promise<ApiResponse<T>> {
      let ret: ApiResponse<T>;
      if(process.env.REACT_APP_PRODUCTION == "0")
        console.info("Doing Http Request: \n\tURL:" + path + "\n\Parameters: " + JSON.stringify(parameters));
      await axiosIntance.get(path, parameters)
        .then((response) => {
          ret = {
            data: response.data,
            httpStatus: response.status.toString(),
            success: true,
            ...ret
          };
        })
        .catch((error) => {
          console.log(`Erro no HTTPClient ${JSON.stringify(error)}`);
          ret = getCatchValue<T>(error, path);
        });
      return ret;
    },
    doPostFormData: async function <T>(path: string, parameters: FormData): Promise<ApiResponse<T>> {
      let ret: ApiResponse<T>;
      if(process.env.REACT_APP_PRODUCTION == "0")
        console.info("Requisição com FormData: \n\tURL:" + path + "\n\tParâmetros: " + JSON.stringify(parameters));

      await axiosIntance.post(path, parameters, {
        headers: {
          'Content-Type': 'multipart/form-data;'
        }
      })
        .then((response) => {
          console.info("Resposta requisição: \n\tURL:" + path + "\n\Response: " + JSON.stringify(response));
          ret = {
            data: response.data,
            httpStatus: response.status.toString(),
            success: true,
            ...ret
          };
        })
        .catch((error) => {
          console.error("Erro na requisição requisição: \n\tURL:" + path + "\n\Error: " + JSON.stringify(error));
          ret = getCatchValue<T>(error, path);
        });
      return ret;
    },
    doPostFormDataAuth: async function <T>(path: string, parameters: FormData, tokenAuth: string): Promise<ApiResponse<T>> {
      let ret: ApiResponse<T>;
      if(process.env.REACT_APP_PRODUCTION == "0")
        console.info("Requisição com FormData: \n\tURL:" + path + "\n\tParâmetros: " + JSON.stringify(parameters));

      await axiosIntance.post(path, parameters, {
        headers: {
          'Content-Type': 'multipart/form-data;',
          "Authorization": "Basic " + tokenAuth
        }
      })
        .then((response) => {
          console.info("Resposta requisição: \n\tURL:" + path + "\n\Response: " + JSON.stringify(response));
          ret = {
            data: response.data,
            httpStatus: response.status.toString(),
            success: true,
            ...ret
          };
        })
        .catch((error) => {
          console.error("Erro na requisição requisição: \n\tURL:" + path + "\n\Error: " + JSON.stringify(error));
          ret = getCatchValue<T>(error, path);
        });
      return ret;
    },
    doPutAuth: async function <T>(path: string, parameters: any, tokenAuth: string): Promise<ApiResponse<T>> {
      let ret: ApiResponse<T>;
      if(process.env.REACT_APP_PRODUCTION == "0")
        console.info("Requisição com token realizada: \n\tURL:" + path + "\n\tParâmetros: " + JSON.stringify(parameters) + "\n\tToken: " + tokenAuth);
      await axiosIntance.put(path, parameters, {
        headers: {
          "Content-Type": "application/json",
          "Authorization": "Bearer " + tokenAuth
        }
      })
        .then((response) => {
          ret = {
            data: response.data,
            httpStatus: response.status.toString(),
            success: true,
            ...ret
          };
        })
        .catch((error) => {
          ret = getCatchValue<T>(error, path);
        });
      return ret;
    }
  }
}

export { HttpClient };