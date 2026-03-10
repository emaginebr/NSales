import React from 'react';
import INetworkProvider from '../../DTO/Contexts/INetworkProvider';

const NetworkContext = React.createContext<INetworkProvider>(null);

export default NetworkContext;