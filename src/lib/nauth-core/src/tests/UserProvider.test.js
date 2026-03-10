import { jsx as _jsx } from "react/jsx-runtime";
import React from 'react';
import { renderHook, act } from '@testing-library/react-hooks';
import UserProvider from '../Contexts/User/UserProvider';
import UserContext from '../Contexts/User/UserContext';
import UserFactory from '../Business/Factory/UserFactory';
import { LanguageEnum } from '../DTO/Enum/LanguageEnum';
jest.mock('../Business/Factory/UserFactory', () => ({
    __esModule: true,
    default: {
        UserBusiness: {
            loginWithEmail: jest.fn(),
            setSession: jest.fn(),
            getSession: jest.fn(),
            getMe: jest.fn(),
            cleanSession: jest.fn(),
            uploadImageUser: jest.fn(),
            getUserByEmail: jest.fn(),
            getBySlug: jest.fn(),
            insert: jest.fn(),
            update: jest.fn(),
            hasPassword: jest.fn(),
            changePassword: jest.fn(),
            sendRecoveryEmail: jest.fn(),
            changePasswordUsingHash: jest.fn(),
            list: jest.fn(),
        },
    },
}));
const loginMock = UserFactory.UserBusiness.loginWithEmail;
const setSessionMock = UserFactory.UserBusiness.setSession;
const getSessionMock = UserFactory.UserBusiness.getSession;
const getMeMock = UserFactory.UserBusiness.getMe;
const cleanSessionMock = UserFactory.UserBusiness.cleanSession;
const uploadImageUserMock = UserFactory.UserBusiness.uploadImageUser;
const getUserByEmailMock = UserFactory.UserBusiness.getUserByEmail;
const getBySlugMock = UserFactory.UserBusiness.getBySlug;
const insertMock = UserFactory.UserBusiness.insert;
const updateMock = UserFactory.UserBusiness.update;
const hasPasswordMock = UserFactory.UserBusiness.hasPassword;
const changePasswordMock = UserFactory.UserBusiness.changePassword;
const sendRecoveryEmailMock = UserFactory.UserBusiness.sendRecoveryEmail;
const changePasswordUsingHashMock = UserFactory.UserBusiness.changePasswordUsingHash;
const listMock = UserFactory.UserBusiness.list;
const wrapper = ({ children }) => (_jsx(UserProvider, { children: children }));
const fakeUser = {
    userId: 1,
    email: 'user@example.com',
    slug: 'user-slug',
    imageUrl: '',
    name: 'User',
    hash: 'hash',
    password: '',
    isAdmin: true,
    birthDate: '',
    idDocument: '',
    pixKey: '',
    phones: [],
    addresses: [],
    createAt: '',
    updateAt: '',
};
describe('UserContext loginWithEmail', () => {
    beforeEach(() => {
        loginMock.mockReset();
    });
    it('should login and update user and session', async () => {
        loginMock.mockResolvedValue({
            sucesso: true,
            mensagem: '',
            dataResult: { token: 'abc', user: fakeUser },
        });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        act(() => {
            result.current.setSession({});
        });
        let response;
        await act(async () => {
            response = await result.current.loginWithEmail('user@example.com', 'secret');
        });
        expect(loginMock).toHaveBeenCalledWith('user@example.com', 'secret');
        expect(response.sucesso).toBe(true);
        expect(result.current.user).toEqual({ token: 'abc', user: fakeUser });
        expect(result.current.sessionInfo).toEqual({
            userId: fakeUser.userId,
            hash: fakeUser.hash,
            token: 'abc',
            isAdmin: fakeUser.isAdmin,
            name: fakeUser.name,
            email: fakeUser.email,
            language: LanguageEnum.English,
        });
        expect(result.current.loading).toBe(false);
    });
    it('should handle login failure', async () => {
        loginMock.mockResolvedValue({
            sucesso: false,
            mensagem: 'Invalid',
        });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        act(() => {
            result.current.setSession({});
        });
        let response;
        await act(async () => {
            response = await result.current.loginWithEmail('user@example.com', 'secret');
        });
        expect(loginMock).toHaveBeenCalledWith('user@example.com', 'secret');
        expect(response.sucesso).toBe(false);
        expect(result.current.user).toBeNull();
        expect(result.current.sessionInfo).toEqual({});
        expect(result.current.loading).toBe(false);
    });
});
describe('UserContext loadUserSession and getMe', () => {
    beforeEach(() => {
        getSessionMock.mockReset();
        getMeMock.mockReset();
        setSessionMock.mockReset();
    });
    it('should load session and fetch user', async () => {
        const session = {
            userId: 1,
            hash: 'hash',
            token: 'token',
            isAdmin: false,
            name: 'User',
            email: 'user@example.com',
            language: LanguageEnum.English,
        };
        getSessionMock.mockResolvedValue(session);
        getMeMock.mockResolvedValue({
            sucesso: true,
            mensagem: '',
            dataResult: fakeUser,
        });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let loadResp;
        await act(async () => {
            loadResp = await result.current.loadUserSession();
        });
        expect(getSessionMock).toHaveBeenCalled();
        expect(loadResp.sucesso).toBe(true);
        expect(setSessionMock).toHaveBeenCalledWith(session);
        expect(result.current.sessionInfo).toEqual(session);
        let meResp;
        await act(async () => {
            meResp = await result.current.getMe();
        });
        expect(getMeMock).toHaveBeenCalled();
        expect(meResp.sucesso).toBe(true);
        expect(meResp.user).toEqual(fakeUser);
        expect(result.current.user).toEqual(fakeUser);
        expect(result.current.loading).toBe(false);
    });
});
describe('UserContext additional methods', () => {
    beforeEach(() => {
        cleanSessionMock.mockReset();
        uploadImageUserMock.mockReset();
        getUserByEmailMock.mockReset();
        getBySlugMock.mockReset();
        insertMock.mockReset();
        updateMock.mockReset();
        hasPasswordMock.mockReset();
        changePasswordMock.mockReset();
        sendRecoveryEmailMock.mockReset();
        changePasswordUsingHashMock.mockReset();
        listMock.mockReset();
    });
    it('should logout and clear session', () => {
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        act(() => {
            result.current.setSession({ token: 'x' });
        });
        let resp;
        act(() => {
            resp = result.current.logout();
        });
        expect(cleanSessionMock).toHaveBeenCalled();
        expect(resp.sucesso).toBe(true);
        expect(result.current.sessionInfo).toBeNull();
    });
    it('should upload user image', async () => {
        const file = new Blob(['avatar']);
        uploadImageUserMock.mockResolvedValue({ sucesso: true, mensagem: '', dataResult: 'img-url' });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.uploadImageUser(file);
        });
        expect(uploadImageUserMock).toHaveBeenCalledWith(file);
        expect(resp).toEqual({ sucesso: true, url: 'img-url', mensagemSucesso: 'Profile added' });
        expect(result.current.loading).toBe(false);
    });
    it('should handle upload image failure', async () => {
        const file = new Blob(['avatar']);
        uploadImageUserMock.mockResolvedValue({ sucesso: false, mensagem: 'error' });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.uploadImageUser(file);
        });
        expect(uploadImageUserMock).toHaveBeenCalledWith(file);
        expect(resp.sucesso).toBe(false);
        expect(resp.mensagemErro).toBe('error');
        expect(result.current.loading).toBe(false);
    });
    it('should get user by email', async () => {
        getUserByEmailMock.mockResolvedValue({ sucesso: true, mensagem: '', dataResult: fakeUser });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.getUserByEmail('user@example.com');
        });
        expect(getUserByEmailMock).toHaveBeenCalledWith('user@example.com');
        expect(resp.sucesso).toBe(true);
        expect(result.current.user).toEqual(fakeUser);
        expect(result.current.loading).toBe(false);
    });
    it('should handle getUserByEmail failure', async () => {
        getUserByEmailMock.mockResolvedValue({ sucesso: false, mensagem: 'not found' });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.getUserByEmail('user@example.com');
        });
        expect(getUserByEmailMock).toHaveBeenCalledWith('user@example.com');
        expect(resp.sucesso).toBe(false);
        expect(resp.mensagemErro).toBe('not found');
        expect(result.current.user).toBeNull();
        expect(result.current.loading).toBe(false);
    });
    it('should get user by slug', async () => {
        getBySlugMock.mockResolvedValue({ sucesso: true, mensagem: '', dataResult: fakeUser });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.getBySlug('user-slug');
        });
        expect(getBySlugMock).toHaveBeenCalledWith('user-slug');
        expect(resp.sucesso).toBe(true);
        expect(result.current.user).toEqual(fakeUser);
        expect(result.current.loading).toBe(false);
    });
    it('should handle getBySlug failure', async () => {
        getBySlugMock.mockResolvedValue({ sucesso: false, mensagem: 'not found' });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.getBySlug('user-slug');
        });
        expect(getBySlugMock).toHaveBeenCalledWith('user-slug');
        expect(resp.sucesso).toBe(false);
        expect(resp.mensagemErro).toBe('not found');
        expect(result.current.user).toBeNull();
        expect(result.current.loading).toBe(false);
    });
    it('should insert user', async () => {
        insertMock.mockResolvedValue({ sucesso: true, mensagem: '', dataResult: fakeUser });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.insert(fakeUser);
        });
        expect(insertMock).toHaveBeenCalledWith(fakeUser);
        expect(resp.sucesso).toBe(true);
        expect(result.current.user).toEqual(fakeUser);
        expect(result.current.loadingUpdate).toBe(false);
    });
    it('should handle insert failure', async () => {
        insertMock.mockResolvedValue({ sucesso: false, mensagem: 'error' });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.insert(fakeUser);
        });
        expect(insertMock).toHaveBeenCalledWith(fakeUser);
        expect(resp.sucesso).toBe(false);
        expect(resp.mensagemErro).toBe('error');
        expect(result.current.user).toBeNull();
        expect(result.current.loadingUpdate).toBe(false);
    });
    it('should update user', async () => {
        var _a;
        updateMock.mockResolvedValue({
            sucesso: true,
            mensagem: '',
            dataResult: Object.assign(Object.assign({}, fakeUser), { name: 'Updated' }),
        });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.update(Object.assign(Object.assign({}, fakeUser), { name: 'Updated' }));
        });
        expect(updateMock).toHaveBeenCalled();
        expect(resp.sucesso).toBe(true);
        expect((_a = result.current.user) === null || _a === void 0 ? void 0 : _a.name).toBe('Updated');
        expect(result.current.loadingUpdate).toBe(false);
    });
    it('should handle update failure', async () => {
        updateMock.mockResolvedValue({ sucesso: false, mensagem: 'error' });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.update(fakeUser);
        });
        expect(updateMock).toHaveBeenCalledWith(fakeUser);
        expect(resp.sucesso).toBe(false);
        expect(resp.mensagemErro).toBe('error');
        expect(result.current.user).toBeNull();
        expect(result.current.loadingUpdate).toBe(false);
    });
    it('should check hasPassword', async () => {
        hasPasswordMock.mockResolvedValue({ sucesso: true });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.hasPassword();
        });
        expect(hasPasswordMock).toHaveBeenCalled();
        expect(resp.sucesso).toBe(true);
        expect(result.current.userHasPassword).toBe(true);
        expect(result.current.loadingPassword).toBe(false);
    });
    it('should handle hasPassword failure', async () => {
        hasPasswordMock.mockResolvedValue({ sucesso: false, mensagem: 'error' });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.hasPassword();
        });
        expect(hasPasswordMock).toHaveBeenCalled();
        expect(resp.sucesso).toBe(false);
        expect(result.current.userHasPassword).toBe(false);
        expect(result.current.loadingPassword).toBe(false);
    });
    it('should change password', async () => {
        changePasswordMock.mockResolvedValue({ sucesso: true, mensagem: 'ok' });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.changePassword('old', 'new');
        });
        expect(changePasswordMock).toHaveBeenCalledWith('old', 'new');
        expect(resp.sucesso).toBe(true);
        expect(resp.mensagemSucesso).toBe('ok');
        expect(result.current.loadingUpdate).toBe(false);
    });
    it('should handle changePassword failure', async () => {
        changePasswordMock.mockResolvedValue({ sucesso: false, mensagem: 'error' });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.changePassword('old', 'new');
        });
        expect(changePasswordMock).toHaveBeenCalledWith('old', 'new');
        expect(resp.sucesso).toBe(false);
        expect(resp.mensagemErro).toBe('error');
        expect(result.current.loadingUpdate).toBe(false);
    });
    it('should send recovery email', async () => {
        sendRecoveryEmailMock.mockResolvedValue({ sucesso: true, mensagem: '' });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.sendRecoveryEmail('user@example.com');
        });
        expect(sendRecoveryEmailMock).toHaveBeenCalledWith('user@example.com');
        expect(resp.sucesso).toBe(true);
        expect(result.current.loadingUpdate).toBe(false);
    });
    it('should handle sendRecoveryEmail failure', async () => {
        sendRecoveryEmailMock.mockResolvedValue({ sucesso: false, mensagem: 'error' });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.sendRecoveryEmail('user@example.com');
        });
        expect(sendRecoveryEmailMock).toHaveBeenCalledWith('user@example.com');
        expect(resp.sucesso).toBe(false);
        expect(resp.mensagemErro).toBe('error');
        expect(result.current.loadingUpdate).toBe(false);
    });
    it('should change password using hash', async () => {
        changePasswordUsingHashMock.mockResolvedValue({ sucesso: true, mensagem: '' });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.changePasswordUsingHash('recovery', 'new');
        });
        expect(changePasswordUsingHashMock).toHaveBeenCalledWith('recovery', 'new');
        expect(resp.sucesso).toBe(true);
        expect(result.current.loadingUpdate).toBe(false);
    });
    it('should handle changePasswordUsingHash failure', async () => {
        changePasswordUsingHashMock.mockResolvedValue({ sucesso: false, mensagem: 'error' });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.changePasswordUsingHash('recovery', 'new');
        });
        expect(changePasswordUsingHashMock).toHaveBeenCalledWith('recovery', 'new');
        expect(resp.sucesso).toBe(false);
        expect(resp.mensagemErro).toBe('error');
        expect(result.current.loadingUpdate).toBe(false);
    });
    it('should list users', async () => {
        listMock.mockResolvedValue({ sucesso: true, mensagem: '', dataResult: [fakeUser] });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.list(5);
        });
        expect(listMock).toHaveBeenCalledWith(5);
        expect(resp.sucesso).toBe(true);
        expect(result.current.users).toEqual([fakeUser]);
        expect(result.current.loadingList).toBe(false);
    });
    it('should handle list failure', async () => {
        listMock.mockResolvedValue({ sucesso: false, mensagem: 'error' });
        const { result } = renderHook(() => React.useContext(UserContext), { wrapper });
        let resp;
        await act(async () => {
            resp = await result.current.list(5);
        });
        expect(listMock).toHaveBeenCalledWith(5);
        expect(resp.sucesso).toBe(false);
        expect(resp.mensagemErro).toBe('error');
        expect(result.current.users).toEqual([]);
        expect(result.current.loadingList).toBe(false);
    });
});
