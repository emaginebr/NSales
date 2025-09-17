import React from 'react';
import IUserProvider from '../../DTO/Contexts/IUserProvider';

const UserContext = React.createContext<IUserProvider>(null);

export default UserContext;