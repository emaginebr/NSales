import React from 'react';
import IAuthProvider from '../../DTO/Contexts/IAuthProvider';

const AuthContext = React.createContext<IAuthProvider>(null);

export default AuthContext;