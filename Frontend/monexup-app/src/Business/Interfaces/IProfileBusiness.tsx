import BusinessResult from "../../DTO/Business/BusinessResult";
import UserProfileInfo from "../../DTO/Domain/UserProfileInfo";
import IProfileService from "../../Services/Interfaces/IProfileService";

export default interface IProfileBusiness {
  init: (profileService: IProfileService) => void;
  insert: (profile: UserProfileInfo) => Promise<BusinessResult<UserProfileInfo>>;
  update: (profile: UserProfileInfo) => Promise<BusinessResult<UserProfileInfo>>;
  delete: (profileId: number) => Promise<BusinessResult<Boolean>>;
  listByNetwork: (networkId: number) => Promise<BusinessResult<UserProfileInfo[]>>;
  getById: (profileId: number) => Promise<BusinessResult<UserProfileInfo>>;
}