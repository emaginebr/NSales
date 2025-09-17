import UserInfo from '../Domain/UserInfo';
import ProviderResult from './ProviderResult';
export default interface UserProviderResult extends ProviderResult {
    user?: UserInfo;
}
