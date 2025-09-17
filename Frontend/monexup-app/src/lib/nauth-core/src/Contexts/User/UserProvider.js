import { jsx as _jsx } from "react/jsx-runtime";
import { useState } from 'react';
import UserContext from './UserContext';
import UserFactory from '../../Business/Factory/UserFactory';
import { LanguageEnum } from '../../DTO/Enum/LanguageEnum';
export default function UserProvider(props) {
    const [loading, setLoading] = useState(false);
    const [loadingList, setLoadingList] = useState(false);
    const [loadingPassword, setLoadingPassword] = useState(false);
    const [loadingUpdate, setLoadingUpdate] = useState(false);
    const [loadingSearch, setLoadingSearch] = useState(false);
    const [language, setLanguage] = useState(LanguageEnum.English);
    const [sessionInfo, _setSessionInfo] = useState(null);
    const [userHasPassword, setUserHasPassword] = useState(false);
    const [userImage, setUserImage] = useState('');
    const [user, _setUser] = useState(null);
    const [users, setUsers] = useState([]);
    const userProviderValue = {
        loading: loading,
        loadingList: loadingList,
        loadingPassword: loadingPassword,
        loadingUpdate: loadingUpdate,
        loadingSearch: loadingSearch,
        userHasPassword: userHasPassword,
        user: user,
        users: users,
        language: language,
        sessionInfo: sessionInfo,
        setSession: (session) => {
            console.log(JSON.stringify(session));
            _setSessionInfo(session);
            UserFactory.UserBusiness.setSession(session);
        },
        setLanguage: (value) => {
            setLanguage(value);
        },
        setUser: (user) => {
            _setUser(user);
        },
        logout: function () {
            try {
                UserFactory.UserBusiness.cleanSession();
                _setSessionInfo(null);
                return {
                    sucesso: true,
                    mensagemErro: '',
                    mensagemSucesso: '',
                };
            }
            catch (err) {
                return {
                    sucesso: false,
                    mensagemErro: 'Falha ao tenta executar o logout',
                    mensagemSucesso: '',
                };
            }
        },
        loadUserSession: async () => {
            const ret = {};
            const session = await UserFactory.UserBusiness.getSession();
            if (session) {
                userProviderValue.setSession(session);
                return Object.assign(Object.assign({}, ret), { sucesso: true });
            }
            return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: 'Session not load' });
        },
        uploadImageUser: async (file) => {
            const ret = {};
            setLoading(true);
            const brt = await UserFactory.UserBusiness.uploadImageUser(file);
            if (brt.sucesso) {
                setLoading(false);
                setUserImage(brt.dataResult);
                return Object.assign(Object.assign({}, ret), { sucesso: true, url: brt.dataResult, mensagemSucesso: 'Profile added' });
            }
            else {
                setLoading(false);
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: brt.mensagem });
            }
        },
        getMe: async () => {
            const ret = {};
            setLoading(true);
            try {
                const brt = await UserFactory.UserBusiness.getMe();
                if (brt.sucesso) {
                    setLoading(false);
                    _setUser(brt.dataResult);
                    return Object.assign(Object.assign({}, ret), { user: brt.dataResult, sucesso: true, mensagemSucesso: 'User load' });
                }
                else {
                    setLoading(false);
                    return Object.assign(Object.assign({}, ret), { user: null, sucesso: false, mensagemErro: brt.mensagem });
                }
            }
            catch (err) {
                setLoading(false);
                return Object.assign(Object.assign({}, ret), { user: null, sucesso: false, mensagemErro: JSON.stringify(err) });
            }
        },
        getUserByEmail: async (email) => {
            const ret = {};
            setLoading(true);
            try {
                const brt = await UserFactory.UserBusiness.getUserByEmail(email);
                if (brt.sucesso) {
                    setLoading(false);
                    _setUser(brt.dataResult);
                    return Object.assign(Object.assign({}, ret), { sucesso: true, mensagemSucesso: 'User load' });
                }
                else {
                    setLoading(false);
                    return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: brt.mensagem });
                }
            }
            catch (err) {
                setLoading(false);
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: JSON.stringify(err) });
            }
        },
        getBySlug: async (slug) => {
            const ret = {};
            setLoading(true);
            try {
                const brt = await UserFactory.UserBusiness.getBySlug(slug);
                if (brt.sucesso) {
                    setLoading(false);
                    _setUser(brt.dataResult);
                    return Object.assign(Object.assign({}, ret), { sucesso: true, mensagemSucesso: 'User load' });
                }
                else {
                    setLoading(false);
                    return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: brt.mensagem });
                }
            }
            catch (err) {
                setLoading(false);
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: JSON.stringify(err) });
            }
        },
        insert: async (user) => {
            const ret = {};
            setLoadingUpdate(true);
            try {
                const brt = await UserFactory.UserBusiness.insert(user);
                if (brt.sucesso) {
                    setLoadingUpdate(false);
                    _setUser(brt.dataResult);
                    return Object.assign(Object.assign({}, ret), { sucesso: true, mensagemSucesso: 'User inseted' });
                }
                else {
                    setLoadingUpdate(false);
                    return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: brt.mensagem });
                }
            }
            catch (err) {
                setLoadingUpdate(false);
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: JSON.stringify(err) });
            }
        },
        update: async (user) => {
            const ret = {};
            setLoadingUpdate(true);
            try {
                const brt = await UserFactory.UserBusiness.update(user);
                if (brt.sucesso) {
                    setLoadingUpdate(false);
                    _setUser(brt.dataResult);
                    return Object.assign(Object.assign({}, ret), { sucesso: true, mensagemSucesso: 'User updated' });
                }
                else {
                    setLoadingUpdate(false);
                    return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: brt.mensagem });
                }
            }
            catch (err) {
                setLoadingUpdate(false);
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: JSON.stringify(err) });
            }
        },
        loginWithEmail: async (email, password) => {
            const ret = {};
            setLoading(true);
            try {
                const bsRet = await UserFactory.UserBusiness.loginWithEmail(email, password);
                if (bsRet.sucesso) {
                    setLoading(false);
                    _setUser(bsRet.dataResult.user);
                    userProviderValue.setSession(Object.assign(Object.assign({}, sessionInfo), { userId: bsRet.dataResult.user.userId, hash: bsRet.dataResult.user.hash, token: bsRet.dataResult.token, isAdmin: bsRet.dataResult.user.isAdmin, name: bsRet.dataResult.user.name, email: bsRet.dataResult.user.email, language: language }));
                    return Object.assign(Object.assign({}, ret), { sucesso: true, mensagemSucesso: 'User successfully logged' });
                }
                else {
                    setLoading(false);
                    return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: bsRet.mensagem });
                }
            }
            catch (err) {
                setLoading(false);
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: JSON.stringify(err) });
            }
        },
        hasPassword: async () => {
            const ret = {};
            setLoadingPassword(true);
            setUserHasPassword(false);
            try {
                const brt = await UserFactory.UserBusiness.hasPassword();
                if (brt.sucesso) {
                    setUserHasPassword(true);
                    setLoadingPassword(false);
                    return Object.assign(Object.assign({}, ret), { sucesso: true, mensagemSucesso: 'Password changed' });
                }
                else {
                    setLoadingPassword(false);
                    return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: brt.mensagem });
                }
            }
            catch (err) {
                setLoadingPassword(false);
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: JSON.stringify(err) });
            }
        },
        changePassword: async (oldPassword, newPassword) => {
            const ret = {};
            setLoadingUpdate(true);
            try {
                const brt = await UserFactory.UserBusiness.changePassword(oldPassword, newPassword);
                console.log('changePassword: ', JSON.stringify(brt));
                if (brt.sucesso) {
                    setLoadingUpdate(false);
                    return Object.assign(Object.assign({}, ret), { sucesso: true, mensagemSucesso: brt.mensagem });
                }
                else {
                    setLoadingUpdate(false);
                    return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: brt.mensagem });
                }
            }
            catch (err) {
                setLoadingUpdate(false);
                console.log('Error change password: ', err);
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: JSON.stringify(err) });
            }
        },
        sendRecoveryEmail: async (email) => {
            const ret = {};
            setLoadingUpdate(true);
            try {
                const brt = await UserFactory.UserBusiness.sendRecoveryEmail(email);
                if (brt.sucesso) {
                    setLoadingUpdate(false);
                    return Object.assign(Object.assign({}, ret), { sucesso: true, mensagemSucesso: 'Recovery email sent successfully' });
                }
                else {
                    setLoadingUpdate(false);
                    return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: brt.mensagem });
                }
            }
            catch (err) {
                setLoadingUpdate(false);
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: JSON.stringify(err) });
            }
        },
        changePasswordUsingHash: async (recoveryHash, newPassword) => {
            const ret = {};
            setLoadingUpdate(true);
            try {
                const brt = await UserFactory.UserBusiness.changePasswordUsingHash(recoveryHash, newPassword);
                if (brt.sucesso) {
                    setLoadingUpdate(false);
                    return Object.assign(Object.assign({}, ret), { sucesso: true, mensagemSucesso: 'Recovery email sent successfully' });
                }
                else {
                    setLoadingUpdate(false);
                    return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: brt.mensagem });
                }
            }
            catch (err) {
                setLoadingUpdate(false);
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: JSON.stringify(err) });
            }
        },
        list: async (take) => {
            const ret = {};
            setLoadingList(true);
            try {
                const brt = await UserFactory.UserBusiness.list(take);
                if (brt.sucesso) {
                    setLoadingList(false);
                    setUsers(brt.dataResult);
                    return Object.assign(Object.assign({}, ret), { sucesso: true });
                }
                else {
                    setLoadingList(false);
                    return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: brt.mensagem });
                }
            }
            catch (err) {
                setLoadingList(false);
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagemErro: JSON.stringify(err) });
            }
        }
    };
    return _jsx(UserContext.Provider, { value: userProviderValue, children: props.children });
}
