import React from 'react';
import IProductProvider from '../../DTO/Contexts/IProductProvider';

const ProductContext = React.createContext<IProductProvider>(null);

export default ProductContext;