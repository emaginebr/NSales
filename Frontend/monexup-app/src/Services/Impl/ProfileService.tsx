import NetworkInfo from "../../DTO/Domain/NetworkInfo";
import UserProfileInfo from "../../DTO/Domain/UserProfileInfo";
import NetworkResult from "../../DTO/Services/NetworkResult";
import StatusRequest from "../../DTO/Services/StatusRequest";
import UserNetworkListResult from "../../DTO/Services/UserNetworkListResult";
import UserProfileListResult from "../../DTO/Services/UserProfileListResult";
import UserProfileResult from "../../DTO/Services/UserProfileResult";
import IHttpClient from "../../Infra/Interface/IHttpClient"; 
import IProfileService from "../Interfaces/IProfileService";

let _httpClient : IHttpClient;

const ProfileService : IProfileService = {
    init: function (htppClient: IHttpClient): void {
        _httpClient = htppClient;
    },
    insert: async (profile: UserProfileInfo, token: string) => {
        let ret: UserProfileResult;
        let request = await _httpClient.doPostAuth<UserProfileResult>("/Profile/insert", profile, token);
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
    update: async (profile: UserProfileInfo, token: string) => {
        let ret: UserProfileResult;
        let request = await _httpClient.doPostAuth<UserProfileResult>("/Profile/update", profile, token);
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
    delete: async (profileId: number, token: string) => {
        let ret: StatusRequest;
        let request = await _httpClient.doGetAuth<StatusRequest>("/Profile/delete/" + profileId, token);
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
    listByNetwork: async (networkId: number, token: string) => {
        let ret: UserProfileListResult;
        let request = await _httpClient.doGetAuth<UserProfileListResult>("/Profile/listByNetwork/" + networkId, token);
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
    getById: async (profileId: number, token: string) => {
        let ret: UserProfileResult;
        let request = await _httpClient.doGetAuth<UserProfileResult>("/Profile/getById/" + profileId, token);
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
    }
}

export default ProfileService;