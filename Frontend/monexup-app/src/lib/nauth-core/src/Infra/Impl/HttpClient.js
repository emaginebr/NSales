import axios from 'axios';
import { getFingerprint } from './fingerprint';
let logoff;
function getCatchValue(error, path) {
    let ret = {};
    if (error.response) {
        console.error('ops! ocorreu um erro na solicitação do endpoint: ' +
            path +
            '\nHttp Status:' +
            error.response.status +
            '\n Descrição: ' +
            JSON.stringify(error.response.data));
        if (error.response.status.toString() === '401') {
            logoff();
            window.location.href = '/login';
        }
        ret = Object.assign({ httpStatus: error.response.status.toString(), success: false, messageError: error.response.data }, ret);
    }
    else if (error.request) {
        console.error('Não foi possível receber nenhuma resposta na solicitação do endpoint: ' + path + '\n Descrição: ' + error.request);
        ret = Object.assign({ httpStatus: '400', success: false, messageError: error.request }, ret);
    }
    else {
        console.error('Erro desconhecido na solicitação do endpoint: ' + path + '\n Descrição: ' + error.message);
        ret = Object.assign({ httpStatus: '400', success: false, messageError: error.message }, ret);
    }
    return ret;
}
const HttpClient = () => {
    let axiosIntance;
    return {
        init: (baseUrl) => {
            axiosIntance = axios.create({
                baseURL: baseUrl,
            });
            axiosIntance.interceptors.request.use(async (config) => {
                const fingerprint = await getFingerprint();
                config.headers['X-Device-Fingerprint'] = fingerprint;
                return config;
            });
        },
        setLogoff: (logoffCallback) => {
            logoff = logoffCallback;
        },
        doPost: async function (path, parameters) {
            let ret = {};
            if (import.meta.env.DEV) {
                console.info('Requisição realizada: \n\tURL:' + path + '\n\tParâmetros: ' + JSON.stringify(parameters));
            }
            await axiosIntance
                .post(path, parameters)
                .then((response) => {
                ret = Object.assign({ data: response.data, httpStatus: response.status.toString(), success: true }, ret);
            })
                .catch((error) => {
                console.log(`Erro no HTTPClient ${JSON.stringify(error)}`);
                ret = getCatchValue(error, path);
            });
            return ret;
        },
        doPostAuth: async function (path, parameters, tokenAuth) {
            let ret = {};
            if (import.meta.env.DEV) {
                console.info('Requisição com token realizada: \n\tURL:' + path + '\n\tParâmetros: ' + JSON.stringify(parameters) + '\n\tToken: ' + tokenAuth);
            }
            await axiosIntance
                .post(path, parameters, {
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: 'Basic ' + tokenAuth,
                },
            })
                .then((response) => {
                ret = Object.assign({ data: response.data, httpStatus: response.status.toString(), success: true }, ret);
            })
                .catch((error) => {
                ret = getCatchValue(error, path);
            });
            return ret;
        },
        doGetAuth: async function (path, tokenAuth) {
            let ret = {};
            if (import.meta.env.DEV) {
                console.info('Requisição com token realizada: \n\tURL:' + path + '\n\tToken: ' + tokenAuth);
            }
            await axiosIntance
                .get(path, {
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: 'Basic ' + tokenAuth,
                },
            })
                .then((response) => {
                ret = Object.assign({ data: response.data, httpStatus: response.status.toString(), success: true }, ret);
            })
                .catch((error) => {
                ret = getCatchValue(error, path);
            });
            return ret;
        },
        doGet: async function (path, parameters) {
            let ret = {};
            if (import.meta.env.DEV) {
                console.info('Doing Http Request: \n\tURL:' + path + '\n\Parameters: ' + JSON.stringify(parameters));
            }
            await axiosIntance
                .get(path, parameters)
                .then((response) => {
                ret = Object.assign({ data: response.data, httpStatus: response.status.toString(), success: true }, ret);
            })
                .catch((error) => {
                console.log(`Erro no HTTPClient ${JSON.stringify(error)}`);
                ret = getCatchValue(error, path);
            });
            return ret;
        },
        doPostFormData: async function (path, parameters) {
            let ret = {};
            if (import.meta.env.DEV) {
                console.info('Requisição com FormData: \n\tURL:' + path + '\n\tParâmetros: ' + JSON.stringify(parameters));
            }
            await axiosIntance
                .post(path, parameters, {
                headers: {
                    'Content-Type': 'multipart/form-data;',
                },
            })
                .then((response) => {
                console.info('Resposta requisição: \n\tURL:' + path + '\n\Response: ' + JSON.stringify(response));
                ret = Object.assign({ data: response.data, httpStatus: response.status.toString(), success: true }, ret);
            })
                .catch((error) => {
                console.error('Erro na requisição requisição: \n\tURL:' + path + '\n\Error: ' + JSON.stringify(error));
                ret = getCatchValue(error, path);
            });
            return ret;
        },
        doPostFormDataAuth: async function (path, parameters, tokenAuth) {
            let ret = {};
            if (import.meta.env.DEV) {
                console.info('Requisição com FormData: \n\tURL:' + path + '\n\tParâmetros: ' + JSON.stringify(parameters));
            }
            await axiosIntance
                .post(path, parameters, {
                headers: {
                    'Content-Type': 'multipart/form-data;',
                    Authorization: 'Basic ' + tokenAuth,
                },
            })
                .then((response) => {
                console.info('Resposta requisição: \n\tURL:' + path + '\n\Response: ' + JSON.stringify(response));
                ret = Object.assign({ data: response.data, httpStatus: response.status.toString(), success: true }, ret);
            })
                .catch((error) => {
                console.error('Erro na requisição requisição: \n\tURL:' + path + '\n\Error: ' + JSON.stringify(error));
                ret = getCatchValue(error, path);
            });
            return ret;
        },
        doPutAuth: async function (path, parameters, tokenAuth) {
            let ret = {};
            if (import.meta.env.DEV) {
                console.info('Requisição com token realizada: \n\tURL:' + path + '\n\tParâmetros: ' + JSON.stringify(parameters) + '\n\tToken: ' + tokenAuth);
            }
            await axiosIntance
                .put(path, parameters, {
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: 'Bearer ' + tokenAuth,
                },
            })
                .then((response) => {
                ret = Object.assign({ data: response.data, httpStatus: response.status.toString(), success: true }, ret);
            })
                .catch((error) => {
                ret = getCatchValue(error, path);
            });
            return ret;
        },
    };
};
export { HttpClient };
