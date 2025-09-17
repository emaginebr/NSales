import { useContext, useEffect, useState } from "react";
import Col from "react-bootstrap/esm/Col";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCalendar, faCancel, faDollar, faEdit, faEnvelope, faPlus, faSearch, faTrash } from '@fortawesome/free-solid-svg-icons';
import Table from "react-bootstrap/esm/Table";
import { Link, useNavigate, useParams } from "react-router-dom";
import Button from 'react-bootstrap/Button';
import Form from 'react-bootstrap/Form';
import InputGroup from 'react-bootstrap/InputGroup';
import Dropdown from 'react-bootstrap/Dropdown';
import Pagination from 'react-bootstrap/Pagination';
import NetworkContext from "../../Contexts/Network/NetworkContext";
import OrderContext from "../../Contexts/Order/OrderContext";
import { MessageToastEnum } from "../../DTO/Enum/MessageToastEnum";
import { OrderStatusEnum } from "../../DTO/Enum/OrderStatusEnum";
import MessageToast from "../../Components/MessageToast";
import { UserRoleEnum } from "../../DTO/Enum/UserRoleEnum";
import AuthContext from "../../Contexts/Auth/AuthContext";
import OrderInfo from "../../DTO/Domain/OrderInfo";
import Moment from 'react-moment';
import { useTranslation } from "react-i18next";

export default function OrderSearchPage() {


    const { t } = useTranslation();

    let navigate = useNavigate();

    const authContext = useContext(AuthContext);
    const networkContext = useContext(NetworkContext);
    const orderContext = useContext(OrderContext);

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
            ret = t('product_unknown'); // Reusing existing key
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

    const searchOrders = (pageNum: number) => {
        switch (networkContext.userNetwork.role) {
            case UserRoleEnum.NetworkManager:
                orderContext.search(networkContext.userNetwork.networkId, 0, 0, pageNum).then((ret) => {
                    if (!ret.sucesso) {
                        throwError(ret.mensagemErro);
                    }
                });
                break;
            case UserRoleEnum.Seller:
                orderContext.search(
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
                orderContext.search(
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

    const showStatus = (status: OrderStatusEnum) => {
        let retorno: string;
        switch (status) {
            case OrderStatusEnum.Incoming:
                retorno = t('order_status_incoming');
                break;
            case OrderStatusEnum.Active:
                retorno = t('order_status_active');
                break;
            case OrderStatusEnum.Suspended:
                retorno = t('order_status_suspended');
                break;
            case OrderStatusEnum.Finished:
                retorno = t('order_status_finished');
                break;
            case OrderStatusEnum.Expired:
                retorno = t('order_status_expired');
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
            searchOrders(pageNumInt);
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
                                    <li className="breadcrumb-item active" aria-current="page">{t('breadcrumb_order_list')}</li>
                                </ol>
                            </nav>
                        </h3>
                    </Col>
                    <Col md="6" style={{ textAlign: "right" }}>
                        {/*}
                        <InputGroup className="pull-right">
                            <Dropdown>
                                <Dropdown.Toggle variant="danger" id="dropdown-basic">
                                    {t('filter_by_all_status')}
                                </Dropdown.Toggle>

                                <Dropdown.Menu>
                                    <Dropdown.Item href="#/action-1">Action</Dropdown.Item>
                                    <Dropdown.Item href="#/action-2">Another action</Dropdown.Item>
                                    <Dropdown.Item href="#/action-3">Something else</Dropdown.Item>
                                </Dropdown.Menu>
                            </Dropdown>
                        </InputGroup>
                        */}
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
                                    <th>{t('table_header_last_change')}</th>
                                    <th>{t('table_header_status')}</th>
                                    <th>{t('table_header_actions')}</th>
                                </tr>
                            </thead>
                            {
                                orderContext.loadingSearch &&
                                <tr>
                                    <td colSpan={7}>
                                        <div className="d-flex justify-content-center">
                                            <div className="spinner-border" role="status">
                                                <span className="visually-hidden">{t('loading')}</span>
                                            </div>
                                        </div>
                                    </td>
                                </tr>
                            }
                            <tbody>
                                {!orderContext.loadingSearch && orderContext.searchResult?.orders.map((order) => {
                                    return (
                                        <tr>
                                            <td>{showProducts(order)}</td>
                                            <td style={{ textAlign: "right" }}>R$ {showTotal(order)}</td>
                                            <td>{order.user?.name}</td>
                                            <td>{order.seller?.name}</td>
                                            <td><Moment fromNow ago interval={60000}>{order.updatedAt}</Moment></td>
                                            <td>{showStatus(order.status)}</td>
                                            <td>
                                                <Link to="/admin/orders">
                                                    <FontAwesomeIcon icon={faCancel} fixedWidth /> {t('action_suspend')}
                                                </Link>
                                            </td>
                                        </tr>
                                    );
                                })}

                            </tbody>
                        </Table>
                    </Col>
                </Row>
                {!orderContext.loadingSearch && orderContext.searchResult &&
                    <Row>
                        <Col md={12} className="text-center">
                            <Pagination className="justify-content-center">
                                <Pagination.First
                                    disabled={!(orderContext.searchResult?.pageNum > 1)}
                                    onClick={() => searchOrders(1)} />
                                <Pagination.Prev
                                    disabled={!(orderContext.searchResult?.pageNum > 1)}
                                    onClick={() => searchOrders(orderContext.searchResult?.pageNum - 1)} />
                                <Pagination.Ellipsis />

                                {orderContext.searchResult?.pageNum - 2 >= 1 &&
                                    <Pagination.Item
                                        onClick={() => searchOrders(orderContext.searchResult?.pageNum - 2)}
                                    >{orderContext.searchResult?.pageNum - 2}</Pagination.Item>
                                }
                                {orderContext.searchResult?.pageNum - 1 >= 1 &&
                                    <Pagination.Item
                                        onClick={() => searchOrders(orderContext.searchResult?.pageNum - 1)}
                                    >{orderContext.searchResult?.pageNum - 1}</Pagination.Item>
                                }
                                <Pagination.Item active>{orderContext.searchResult?.pageNum}</Pagination.Item>
                                {orderContext.searchResult?.pageNum + 1 <= orderContext.searchResult?.pageCount &&
                                    <Pagination.Item
                                        onClick={() => searchOrders(orderContext.searchResult?.pageNum + 1)}
                                    >{orderContext.searchResult?.pageNum + 1}</Pagination.Item>
                                }
                                {orderContext.searchResult?.pageNum + 2 <= orderContext.searchResult?.pageCount &&
                                    <Pagination.Item
                                        onClick={() => searchOrders(orderContext.searchResult?.pageNum + 2)}
                                    >{orderContext.searchResult?.pageNum + 2}</Pagination.Item>
                                }

                                <Pagination.Ellipsis />
                                <Pagination.Next
                                    disabled={!(orderContext.searchResult?.pageNum < orderContext.searchResult?.pageCount)}
                                    onClick={() => searchOrders(orderContext.searchResult?.pageCount)}
                                />
                                <Pagination.Last
                                    disabled={!(orderContext.searchResult?.pageNum < orderContext.searchResult?.pageCount)}
                                    onClick={() => searchOrders(orderContext.searchResult?.pageCount)} />
                            </Pagination>
                        </Col>
                    </Row>
                }
            </Container>
        </>
    );
}