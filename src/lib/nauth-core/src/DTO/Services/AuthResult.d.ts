import AuthSession from '../Domain/AuthSession';
import StatusRequest from './StatusRequest';
export default interface AuthResult extends StatusRequest {
    user?: AuthSession;
}
