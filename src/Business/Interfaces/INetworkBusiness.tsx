import BusinessResult from "../../DTO/Business/BusinessResult";
import NetworkInfo from "../../DTO/Domain/NetworkInfo";
import NetworkInsertInfo from "../../DTO/Domain/NetworkInsertInfo";
import UserNetworkInfo from "../../DTO/Domain/UserNetworkInfo";
import INetworkService from "../../Services/Interfaces/INetworkService";

export default interface INetworkBusiness {
  init: (networkService: INetworkService) => void;
  insert: (network: NetworkInsertInfo) => Promise<BusinessResult<NetworkInfo>>;
  update: (network: NetworkInfo) => Promise<BusinessResult<NetworkInfo>>;
  listAll: () => Promise<BusinessResult<NetworkInfo[]>>;
  listByUser: () => Promise<BusinessResult<UserNetworkInfo[]>>;
  listByNetwork: (networkSlug: string) => Promise<BusinessResult<UserNetworkInfo[]>>;
  getById: (networkId: number) => Promise<BusinessResult<NetworkInfo>>;
  getBySlug: (networkSlug: string) => Promise<BusinessResult<NetworkInfo>>;
  getUserNetwork: (networkId: number) => Promise<BusinessResult<UserNetworkInfo>>;
  getUserNetworkBySlug: (networkSlug: string) => Promise<BusinessResult<UserNetworkInfo>>;
  getSellerBySlug: (networkSlug: string, userSlug: string) => Promise<BusinessResult<UserNetworkInfo>>;
  requestAccess: (networkId: number, referrerId?: number) => Promise<BusinessResult<boolean>>;
  changeStatus: (networkId: number, userId: number, status: number) => Promise<BusinessResult<boolean>>; 
  promote: (networkId: number, userId: number) => Promise<BusinessResult<boolean>>;
  demote: (networkId: number, userId: number) => Promise<BusinessResult<boolean>>;
}