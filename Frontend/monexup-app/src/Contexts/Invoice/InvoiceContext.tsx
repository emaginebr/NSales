import React from 'react';
import IInvoiceProvider from '../../DTO/Contexts/IInvoiceProvider';

const InvoiceContext = React.createContext<IInvoiceProvider>(null);

export default InvoiceContext;