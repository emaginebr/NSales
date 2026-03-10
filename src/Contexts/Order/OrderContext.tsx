import React from 'react';
import IOrderProvider from '../../DTO/Contexts/IOrderProvider';

const OrderContext = React.createContext<IOrderProvider>(null);

export default OrderContext;