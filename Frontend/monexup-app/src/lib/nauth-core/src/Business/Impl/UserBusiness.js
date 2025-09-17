import UserFactory from '../Factory/UserFactory';
let _userService;
const LS_KEY = 'login-with-metamask:auth';
const UserBusiness = {
    init: function (userService) {
        _userService = userService;
    },
    getSession: () => {
        const ls = window.localStorage.getItem(LS_KEY);
        return ls && JSON.parse(ls);
    },
    setSession: (session) => {
        console.log('Set Session: ', JSON.stringify(session));
        localStorage.setItem(LS_KEY, JSON.stringify(session));
    },
    cleanSession: () => {
        localStorage.removeItem(LS_KEY);
    },
    uploadImageUser: async (file) => {
        try {
            const ret = {};
            const session = UserFactory.UserBusiness.getSession();
            if (!session) {
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: 'Not logged' });
            }
            const retServ = await _userService.uploadImageUser(file, session.token);
            if (retServ.sucesso) {
                return Object.assign(Object.assign({}, ret), { dataResult: retServ.value, sucesso: true });
            }
            else {
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: retServ.mensagem });
            }
        }
        catch (_a) {
            throw new Error('Failed to get user by email');
        }
    },
    getMe: async () => {
        try {
            const ret = {};
            const session = UserFactory.UserBusiness.getSession();
            if (!session) {
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: 'Not logged' });
            }
            const retServ = await _userService.getMe(session.token);
            if (retServ.sucesso) {
                return Object.assign(Object.assign({}, ret), { dataResult: retServ.user, sucesso: true });
            }
            else {
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: retServ.mensagem });
            }
        }
        catch (_a) {
            throw new Error('Failed to get user by address');
        }
    },
    getUserByEmail: async (email) => {
        try {
            const ret = {};
            const retServ = await _userService.getUserByEmail(email);
            if (retServ.sucesso) {
                return Object.assign(Object.assign({}, ret), { dataResult: retServ.user, sucesso: true });
            }
            else {
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: retServ.mensagem });
            }
        }
        catch (_a) {
            throw new Error('Failed to get user by email');
        }
    },
    getBySlug: async (slug) => {
        try {
            const ret = {};
            const retServ = await _userService.getBySlug(slug);
            if (retServ.sucesso) {
                return Object.assign(Object.assign({}, ret), { dataResult: retServ.user, sucesso: true });
            }
            else {
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: retServ.mensagem });
            }
        }
        catch (_a) {
            throw new Error('Failed to get user by email');
        }
    },
    insert: async (user) => {
        try {
            const ret = {};
            const retServ = await _userService.insert(user);
            if (retServ.sucesso) {
                return Object.assign(Object.assign({}, ret), { dataResult: retServ.user, sucesso: true });
            }
            else {
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: retServ.mensagem });
            }
        }
        catch (_a) {
            throw new Error('Failed to insert');
        }
    },
    update: async (user) => {
        try {
            const ret = {};
            const session = UserFactory.UserBusiness.getSession();
            if (!session) {
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: 'Not logged' });
            }
            const retServ = await _userService.update(user, session.token);
            if (retServ.sucesso) {
                return Object.assign(Object.assign({}, ret), { dataResult: retServ.user, sucesso: true });
            }
            else {
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: retServ.mensagem });
            }
        }
        catch (_a) {
            throw new Error('Failed to update');
        }
    },
    loginWithEmail: async (email, password) => {
        try {
            const ret = {};
            const retServ = await _userService.loginWithEmail(email, password);
            if (retServ.sucesso) {
                const userTokenInfo = {
                    token: retServ.token,
                    user: retServ.user
                };
                return Object.assign(Object.assign({}, ret), { dataResult: userTokenInfo, sucesso: true });
            }
            else {
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: retServ.mensagem });
            }
        }
        catch (_a) {
            throw new Error('Failed to login with email');
        }
    },
    hasPassword: async () => {
        try {
            const ret = {};
            const session = UserFactory.UserBusiness.getSession();
            if (!session) {
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: 'Not logged' });
            }
            const retServ = await _userService.hasPassword(session.token);
            if (retServ.sucesso) {
                return Object.assign(Object.assign({}, ret), { dataResult: true, sucesso: true });
            }
            else {
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: retServ.mensagem });
            }
        }
        catch (_a) {
            throw new Error('Failed to change password');
        }
    },
    changePassword: async (oldPassword, newPassword) => {
        const ret = {};
        const session = UserFactory.UserBusiness.getSession();
        if (!session) {
            return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: 'Not logged' });
        }
        const retServ = await _userService.changePassword(oldPassword, newPassword, session.token);
        if (retServ.sucesso) {
            return Object.assign(Object.assign({}, ret), { dataResult: true, sucesso: true, mensagem: retServ.mensagem });
        }
        else {
            return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: retServ.mensagem });
        }
    },
    sendRecoveryEmail: async (email) => {
        try {
            const ret = {};
            const retServ = await _userService.sendRecoveryEmail(email);
            if (retServ.sucesso) {
                return Object.assign(Object.assign({}, ret), { dataResult: ret.sucesso, sucesso: true });
            }
            else {
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: retServ.mensagem });
            }
        }
        catch (_a) {
            throw new Error('Failed to send recovery email');
        }
    },
    changePasswordUsingHash: async (recoveryHash, newPassword) => {
        try {
            const ret = {};
            const retServ = await _userService.changePasswordUsingHash(recoveryHash, newPassword);
            if (retServ.sucesso) {
                return Object.assign(Object.assign({}, ret), { dataResult: ret.sucesso, sucesso: true });
            }
            else {
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: retServ.mensagem });
            }
        }
        catch (_a) {
            throw new Error('Failed to change password using hash');
        }
    },
    list: async (take) => {
        try {
            const ret = {};
            const retServ = await _userService.list(take);
            if (retServ.sucesso) {
                return Object.assign(Object.assign({}, ret), { dataResult: retServ.users, sucesso: true });
            }
            else {
                return Object.assign(Object.assign({}, ret), { sucesso: false, mensagem: retServ.mensagem });
            }
        }
        catch (_a) {
            throw new Error('Failed to get user by email');
        }
    },
    /*,
    search: async (networkId: number, keyword: string, pageNum: number, profileId?: number) => {
        let ret = {} as BusinessResult<UserListPagedInfo>;
        let session: AuthSession = AuthFactory.AuthBusiness.getSession();
        if (!session) {
          return {
            ...ret,
            sucesso: false,
            mensagem: "Not logged"
          };
        }
        let retServ = await _userService.search(networkId, keyword, pageNum, session.token, profileId);
        if (retServ.sucesso) {
          let dataResult: UserListPagedInfo;
          return {
            ...ret,
            dataResult: {
              ...dataResult,
              users: retServ.users,
              pageNum: retServ.pageNum,
              pageCount: retServ.pageCount
            },
            sucesso: true
          };
        } else {
          return {
            ...ret,
            sucesso: false,
            mensagem: retServ.mensagem
          };
        }
    }
        */
};
export default UserBusiness;
