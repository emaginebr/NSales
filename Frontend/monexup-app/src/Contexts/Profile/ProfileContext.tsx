import React from 'react';
import IProfileProvider from '../../DTO/Contexts/IProfileProvider';

const ProfileContext = React.createContext<IProfileProvider>(null);

export default ProfileContext;