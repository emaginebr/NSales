import { useContext, useEffect, useState } from "react";
import Col from "react-bootstrap/esm/Col";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCancel, faSync } from '@fortawesome/free-solid-svg-icons';
import Table from "react-bootstrap/esm/Table";
import { Link, useNavigate, useParams } from "react-router-dom";
import InputGroup from 'react-bootstrap/InputGroup';
import Dropdown from 'react-bootstrap/Dropdown';
import Pagination from 'react-bootstrap/Pagination';
import NetworkContext from "../../Contexts/Network/NetworkContext";
import { MessageToastEnum } from "../../DTO/Enum/MessageToastEnum";
import MessageToast from "../../Components/MessageToast";
import { UserRoleEnum } from "../../DTO/Enum/UserRoleEnum";
import AuthContext from "../../Contexts/Auth/AuthContext";
import OrderInfo from "../../DTO/Domain/OrderInfo";
import Moment from 'react-moment';
import InvoiceContext from "../../Contexts/Invoice/InvoiceContext";
import { InvoiceStatusEnum } from "../../DTO/Enum/InvoiceStatusEnum";
import { useTranslation } from "react-i18next";
import Button from "react-bootstrap/esm/Button";

export default function InvoiceSearchPage() {


    const { t } = useTranslation();

    let navigate = useNavigate();

    const authContext = useContext(AuthContext);
    const networkContext = useContext(NetworkContext);
    const invoiceContext = useContext(InvoiceContext);

    let { pageNum } = useParams();

    const [dialog, setDialog] = useState<MessageToastEnum>(MessageToastEnum.Error);
    const [showMessage, setShowMessage] = useState<boolean>(false);
    const [messageText, setMessageText] = useState<string>("");

    const throwError = (message: string) => {
        setDialog(MessageToastEnum.Error);
        setMessageText(message);
        setShowMessage(true);
    };
    const showSuccessMessage = (message: string) => {
        setDialog(MessageToastEnum.Success);
        setMessageText(message);
        setShowMessage(true);
    };

    const showProducts = (order: OrderInfo) => {
        let ret: string = "";
        if (order.items) {
            order.items.map((item) => {
                if (item.product) {
                    ret = ret + item.product.name + " (" + item.quantity + "), ";
                }
            });
            if (ret.length > 0) {
                ret = ret.substring(0, ret.length - 2);
            }

        }
        if (ret.length == 0) {
            ret = t('product_unknown');
        }
        return ret;
    };

    const showTotal = (order: OrderInfo) => {
        let total: number = 0;
        if (order.items) {
            order.items.map((item) => {
                total += item.product.price * item.quantity;
            });
        }
        return total;
    };

    const searchInvoices = (pageNum: number) => {
        switch (networkContext.currentRole) {
            case UserRoleEnum.NetworkManager:
                invoiceContext.search(
                    networkContext.userNetwork.networkId,
                    0,
                    0,
                    pageNum
                ).then((ret) => {
                    if (!ret.sucesso) {
                        throwError(ret.mensagemErro);
                    }
                });
                break;
            case UserRoleEnum.Seller:
                invoiceContext.search(
                    networkContext.userNetwork.networkId,
                    0,
                    authContext.sessionInfo?.userId,
                    pageNum
                ).then((ret) => {
                    if (!ret.sucesso) {
                        throwError(ret.mensagemErro);
                    }
                });
                break;
            case UserRoleEnum.User:
                invoiceContext.search(
                    networkContext.userNetwork.networkId,
                    authContext.sessionInfo?.userId,
                    0,
                    pageNum
                ).then((ret) => {
                    if (!ret.sucesso) {
                        throwError(ret.mensagemErro);
                    }
                });
                break;
        }
    };

    const showStatus = (status: InvoiceStatusEnum) => {
        let retorno: string;
        switch (status) {
            case InvoiceStatusEnum.Draft:
                retorno = t('invoice_status_draft');
                break;
            case InvoiceStatusEnum.Open:
                retorno = t('invoice_status_open');
                break;
            case InvoiceStatusEnum.Paid:
                retorno = t('invoice_status_paid');
                break;
            case InvoiceStatusEnum.Cancelled:
                retorno = t('invoice_status_cancelled');
                break;
            case InvoiceStatusEnum.Lost:
                retorno = t('invoice_status_lost');
                break;
        }
        return retorno!;
    };

    useEffect(() => {
        if (networkContext.userNetwork) {
            let pageNumInt: number = parseInt(pageNum);
            if (!pageNumInt) {
                pageNumInt = 1;
            }
            searchInvoices(pageNumInt);
        }
    }, []);


    return (
        <>
            <MessageToast
                dialog={dialog}
                showMessage={showMessage}
                messageText={messageText}
                onClose={() => setShowMessage(false)}
            ></MessageToast>
            <Container>
                <Row>
                    <Col md="6">
                        <h3>
                            <nav aria-label="breadcrumb">
                                <ol className="breadcrumb">
                                    <li className="breadcrumb-item"><Link to="/admin/dashboard">{t('breadcrumb_my_network')}</Link></li>
                                    <li className="breadcrumb-item active" aria-current="page">{t('breadcrumb_invoice_search')}</li>
                                </ol>
                            </nav>
                        </h3>
                    </Col>
                    <Col md="6" className="d-flex justify-content-end">
                        <InputGroup className="w-auto">
                            <Button variant={invoiceContext.loadingUpdate ? "danger" : "success"} size="sm" onClick={async () => {
                                let ret = await invoiceContext.syncronize();
                                if (!ret.sucesso) {
                                    throwError(ret.mensagemErro);
                                    return;
                                }
                                searchInvoices(1);
                            }}>
                                {invoiceContext.loadingUpdate ?
                                    <>
                                        <FontAwesomeIcon icon={faSync} fixedWidth /> {t('loading')}
                                    </>
                                    :
                                    <>
                                        <FontAwesomeIcon icon={faSync} fixedWidth /> Syncronize
                                    </>
                                }
                            </Button>
                        </InputGroup>
                    </Col>
                </Row>
                <Row className="py-4">
                    <Col md="12">
                        <Table striped bordered hover>
                            <thead>
                                <tr>
                                    <th>{t('table_header_product')}</th>
                                    <th style={{ textAlign: "right" }}>{t('table_header_price')}</th>
                                    <th>{t('table_header_buyer')}</th>
                                    <th>{t('table_header_seller')}</th>
                                    <th>{t('table_header_due_date')}</th>
                                    <th>{t('table_header_paid_date')}</th>
                                    <th>{t('table_header_status')}</th>
                                    <th>{t('table_header_actions')}</th>
                                </tr>
                            </thead>
                            {
                                invoiceContext.loadingSearch &&
                                <tr>
                                    <td colSpan={8}>
                                        <div className="d-flex justify-content-center">
                                            <div className="spinner-border" role="status">
                                                <span className="visually-hidden">{t('loading')}</span>
                                            </div>
                                        </div>
                                    </td>
                                </tr>
                            }
                            <tbody>
                                {!invoiceContext.loadingSearch && invoiceContext.searchResult &&
                                    <>
                                        {invoiceContext.searchResult.invoices.length == 0 &&
                                            <tr>
                                                <td colSpan={8}>
                                                    <div className="d-flex justify-content-center">
                                                        {t('invoice_search_no_invoices_found')}
                                                    </div>
                                                </td>
                                            </tr>
                                        }
                                        {invoiceContext.searchResult.invoices.map((invoice) => {
                                            return (
                                                <tr>
                                                    <td>{showProducts(invoice.order)}</td>
                                                    <td style={{ textAlign: "right" }}>R$ {showTotal(invoice.order)}</td>
                                                    <td>{invoice.user?.name}</td>
                                                    <td>{invoice.seller?.name}</td>
                                                    <td><Moment format="DD/MM/YYYY" interval={0}>{invoice.dueDate}</Moment></td>
                                                    <td><Moment format="DD/MM/YYYY" interval={0}>{invoice.paymentDate}</Moment></td>
                                                    <td>{showStatus(invoice.status)}</td>
                                                    <td>
                                                        <Link to="/admin/orders">
                                                            <FontAwesomeIcon icon={faCancel} fixedWidth /> {t('action_suspend')}
                                                        </Link>
                                                    </td>
                                                </tr>
                                            );
                                        })}
                                    </>
                                }

                            </tbody>
                        </Table>
                    </Col>
                </Row>
                {!invoiceContext.loadingSearch && invoiceContext.searchResult &&
                    <Row>
                        <Col md={12} className="text-center">
                            <Pagination className="justify-content-center">
                                <Pagination.First
                                    disabled={!(invoiceContext.searchResult?.pageNum > 1)}
                                    onClick={() => searchInvoices(1)} />
                                <Pagination.Prev
                                    disabled={!(invoiceContext.searchResult?.pageNum > 1)}
                                    onClick={() => searchInvoices(invoiceContext.searchResult?.pageNum - 1)} />
                                <Pagination.Ellipsis />

                                {invoiceContext.searchResult?.pageNum - 2 >= 1 &&
                                    <Pagination.Item
                                        onClick={() => searchInvoices(invoiceContext.searchResult?.pageNum - 2)}
                                    >{invoiceContext.searchResult?.pageNum - 2}</Pagination.Item>
                                }
                                {invoiceContext.searchResult?.pageNum - 1 >= 1 &&
                                    <Pagination.Item
                                        onClick={() => searchInvoices(invoiceContext.searchResult?.pageNum - 1)}
                                    >{invoiceContext.searchResult?.pageNum - 1}</Pagination.Item>
                                }
                                <Pagination.Item active>{invoiceContext.searchResult?.pageNum}</Pagination.Item>
                                {invoiceContext.searchResult?.pageNum + 1 <= invoiceContext.searchResult?.pageCount &&
                                    <Pagination.Item
                                        onClick={() => searchInvoices(invoiceContext.searchResult?.pageNum + 1)}
                                    >{invoiceContext.searchResult?.pageNum + 1}</Pagination.Item>
                                }
                                {invoiceContext.searchResult?.pageNum + 2 <= invoiceContext.searchResult?.pageCount &&
                                    <Pagination.Item
                                        onClick={() => searchInvoices(invoiceContext.searchResult?.pageNum + 2)}
                                    >{invoiceContext.searchResult?.pageNum + 2}</Pagination.Item>
                                }

                                <Pagination.Ellipsis />
                                <Pagination.Next
                                    disabled={!(invoiceContext.searchResult?.pageNum < invoiceContext.searchResult?.pageCount)}
                                    onClick={() => searchInvoices(invoiceContext.searchResult?.pageCount)}
                                />
                                <Pagination.Last
                                    disabled={!(invoiceContext.searchResult?.pageNum < invoiceContext.searchResult?.pageCount)}
                                    onClick={() => searchInvoices(invoiceContext.searchResult?.pageCount)} />
                            </Pagination>
                        </Col>
                    </Row>
                }
            </Container>
        </>
    );
}