import NetworkInfo from "../../DTO/Domain/NetworkInfo";
import NetworkInsertInfo from "../../DTO/Domain/NetworkInsertInfo";
import NetworkListResult from "../../DTO/Services/NetworkListResult";
import NetworkResult from "../../DTO/Services/NetworkResult";
import StatusRequest from "../../DTO/Services/StatusRequest";
import UserNetworkListResult from "../../DTO/Services/UserNetworkListResult";
import UserNetworkResult from "../../DTO/Services/UserNetworkResult";
import UserResult from "../../DTO/Services/UserResult";
import UserTokenResult from "../../DTO/Services/UserTokenResult";
import IHttpClient from "../../Infra/Interface/IHttpClient"; 
import INetworkService from "../Interfaces/INetworkService";

let _httpClient : IHttpClient;

const NetworkService : INetworkService = {
    init: function (htppClient: IHttpClient): void {
        _httpClient = htppClient;
    },
    insert: async (network: NetworkInsertInfo, token: string) => {
        let ret: NetworkResult;
        let request = await _httpClient.doPostAuth<NetworkResult>("/Network/insert", network, token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    update: async (network: NetworkInfo, token: string) => {
        let ret: NetworkResult;
        let request = await _httpClient.doPostAuth<NetworkResult>("/Network/update", network, token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    listAll: async () => {
        let ret: NetworkListResult;
        let request = await _httpClient.doGet<NetworkListResult>("/Network/listAll", {});
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    listByUser: async (token: string) => {
        let ret: UserNetworkListResult;
        let request = await _httpClient.doGetAuth<UserNetworkListResult>("/Network/listByUser", token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    listByNetwork: async (networkSlug: string) => {
        let ret: UserNetworkListResult;
        let request = await _httpClient.doGet<UserNetworkListResult>("/Network/listByNetwork/" + networkSlug, {});
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    getById: async (networkId: number, token: string) => {
        let ret: NetworkResult;
        let request = await _httpClient.doGetAuth<NetworkResult>("/Network/getById/" + networkId, token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    getBySlug: async (networkSlug: string) => {
        let ret: NetworkResult;
        let request = await _httpClient.doGet<NetworkResult>("/Network/getBySlug/" + networkSlug, {});
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    getUserNetwork: async (networkId: number, token: string) => {
        let ret: UserNetworkResult;
        let request = await _httpClient.doGetAuth<UserNetworkResult>("/Network/getUserNetwork/" + networkId, token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    getUserNetworkBySlug: async (networkSlug: string, token: string) => {
        let ret: UserNetworkResult;
        let request = await _httpClient.doGetAuth<UserNetworkResult>("/Network/getUserNetworkBySlug/" + networkSlug, token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    getSellerBySlug: async (networkSlug: string, userSlug: string) => {
        let ret: UserNetworkResult;
        let request = await _httpClient.doGet<UserNetworkResult>("/Network/getSellerBySlug/" + networkSlug + "/" + userSlug, {});
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    requestAccess: async (networkId: number, token: string, referrerId?: number) => {
        let ret: StatusRequest;
        console.log(JSON.stringify({
            networkId: networkId,
            referrerId: referrerId
        }));
        let request = await _httpClient.doPostAuth<StatusRequest>("/Network/requestAccess", {
            networkId: networkId,
            referrerId: referrerId
        }, token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    changeStatus: async (networkId: number, userId: number, status: number, token: string) => {
        let ret: StatusRequest;
        let request = await _httpClient.doPostAuth<StatusRequest>("/Network/changeStatus", {
            networkId: networkId,
            userId: userId,
            status: status
        }, token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    promote: async (networkId: number, userId: number, token: string) => {
        let ret: StatusRequest;
        let request = await _httpClient.doGetAuth<StatusRequest>("/Network/promote/" + networkId + "/" + userId, token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
    demote: async (networkId: number, userId: number, token: string) => {
        let ret: StatusRequest;
        let request = await _httpClient.doGetAuth<StatusRequest>("/Network/demote/" + networkId + "/" + userId, token);
        if (request.success) {
            return request.data;
        }
        else {
            ret = {
                mensagem: request.messageError,
                sucesso: false,
                ...ret
            };
        }
        return ret;
    },
}

export default NetworkService;