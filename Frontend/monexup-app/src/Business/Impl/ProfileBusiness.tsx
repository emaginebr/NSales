import BusinessResult from "../../DTO/Business/BusinessResult";
import AuthSession from "../../DTO/Domain/AuthSession";
import NetworkInfo from "../../DTO/Domain/NetworkInfo";
import NetworkInsertInfo from "../../DTO/Domain/NetworkInsertInfo";
import UserNetworkInfo from "../../DTO/Domain/UserNetworkInfo";
import UserProfileInfo from "../../DTO/Domain/UserProfileInfo";
import INetworkService from "../../Services/Interfaces/INetworkService";
import IProfileService from "../../Services/Interfaces/IProfileService";
import AuthFactory from "../Factory/AuthFactory";
import INetworkBusiness from "../Interfaces/INetworkBusiness";
import IProfileBusiness from "../Interfaces/IProfileBusiness";

let _profileService: IProfileService;

const ProfileBusiness: IProfileBusiness = {
  init: function (profileService: IProfileService): void {
    _profileService = profileService;
  },
  insert: async (profile: UserProfileInfo) => {
    //try {
        let ret: BusinessResult<UserProfileInfo>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _profileService.insert(profile, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.profile,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      /*  
      } catch {
        throw new Error("Failed to insert");
      }
      */
  },
  update: async (profile: UserProfileInfo) => {
    try {
        let ret: BusinessResult<UserProfileInfo>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _profileService.update(profile, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.profile,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      } catch {
        throw new Error("Failed to update");
      }
  },
  delete: async (profileId: number) => {
    try {
        let ret: BusinessResult<Boolean>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _profileService.delete(profileId, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.sucesso,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      } catch {
        throw new Error("Failed to update");
      }
  },
  listByNetwork: async (networkId: number) => {
    try {
        let ret: BusinessResult<UserProfileInfo[]>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _profileService.listByNetwork(networkId, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.profiles,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      } catch {
        throw new Error("Failed to get user by email");
      }
  },
  getById: async (profileId: number) => {
    try {
        let ret: BusinessResult<UserProfileInfo>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _profileService.getById(profileId, session.token);
        if (retServ.sucesso) {
          return {
            ...ret,
            dataResult: retServ.profile,
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
      } catch {
        throw new Error("Failed to get user by email");
      }
  }
}

export default ProfileBusiness;