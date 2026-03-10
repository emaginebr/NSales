import ServiceFactory from '../../Services/ServiceFactory';
import InvoiceBusiness from '../Impl/InvoiceBusiness';
import IInvoiceBusiness from '../Interfaces/IInvoiceBusiness';

const invoiceService = ServiceFactory.InvoiceService;

const invoiceBusinessImpl: IInvoiceBusiness = InvoiceBusiness;
invoiceBusinessImpl.init(invoiceService);

const InvoiceFactory = {
  InvoiceBusiness: invoiceBusinessImpl
};

export default InvoiceFactory;
