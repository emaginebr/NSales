import { useContext, useEffect, useState } from "react";
import AuthContext from "../../Contexts/Auth/AuthContext";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import Col from "react-bootstrap/esm/Col";
import Card from "react-bootstrap/esm/Card";
import Alert from 'react-bootstrap/Alert';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faWarning, faPlus, faBurn, faFire, faSearch, faDollar, faClock, faBoltLightning, faLock, faFileUpload, faCalendar, faCalendarAlt, faFileWord, faBoxOpen, faSign, faLockOpen, faUserDoctor, faChartLine, faChartPie, faCoins, faArrowRight, faUserGroup, faBox, faCog, faCogs, faUserCog, faList, faUser } from '@fortawesome/free-solid-svg-icons';
import Button from "react-bootstrap/esm/Button";
import { useNavigate } from "react-router-dom";
import { faBitcoin, faOpencart } from "@fortawesome/free-brands-svg-icons";
import CardHeader from "react-bootstrap/esm/CardHeader";
import CardTitle from "react-bootstrap/esm/CardTitle";
import CardBody from "react-bootstrap/esm/CardBody";
import CardText from "react-bootstrap/esm/CardText";
import Table from "react-bootstrap/esm/Table";
import ListGroup from 'react-bootstrap/ListGroup';
import Badge from 'react-bootstrap/Badge';
import Tab from 'react-bootstrap/Tab';
import Tabs from 'react-bootstrap/Tabs';
import NetworkContext from "../../Contexts/Network/NetworkContext";
import { UserRoleEnum } from "../../DTO/Enum/UserRoleEnum";
import CountPart from "./CountPart";
import InvoiceContext from "../../Contexts/Invoice/InvoiceContext";
import MessageToast from "../../Components/MessageToast";
import { MessageToastEnum } from "../../DTO/Enum/MessageToastEnum";
import Skeleton from "react-loading-skeleton";
import StatementPart from "./StatementPart";
import StatementSearchParam from "../../DTO/Domain/StatementSearchParam";
import { useTranslation } from "react-i18next";

export default function DashboardPage() {

    const authContext = useContext(AuthContext);
    const networkContext = useContext(NetworkContext);
    const invoiceContext = useContext(InvoiceContext);

    const { t } = useTranslation();

    let navigate = useNavigate();

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

    const searchStatements = async (pageNum: number) => {
        let param: StatementSearchParam;
        switch (networkContext.currentRole) {
            case UserRoleEnum.NetworkManager:
                param = {
                    ...param,
                    networkId: networkContext.userNetwork.networkId,
                    pageNum: 1
                };
                break;
            case UserRoleEnum.Seller:
                param = {
                    ...param,
                    userId: authContext.sessionInfo.userId,
                    pageNum: 1
                };
                break;
        }
        if (networkContext.currentRole != UserRoleEnum.User) {
            var ret = await invoiceContext.searchStatement(param);
            if (!ret.sucesso) {
                throwError(ret.mensagemErro);
            }
        }
    };

    useEffect(() => {
        searchStatements(1);
        switch (networkContext.currentRole) {
            case UserRoleEnum.NetworkManager:
                invoiceContext.getBalance(networkContext.userNetwork.networkId).then((ret) => {
                    if (!ret.sucesso) {
                        throwError(ret.mensagemErro);
                    }
                });
                break;
            case UserRoleEnum.Seller:
                invoiceContext.getBalance().then((ret) => {
                    if (!ret.sucesso) {
                        throwError(ret.mensagemErro);
                    }
                });
                invoiceContext.getAvailableBalance().then((ret) => {
                    if (!ret.sucesso) {
                        throwError(ret.mensagemErro);
                    }
                });
                break;
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
                {networkContext.currentRole != UserRoleEnum.User &&
                    <Row>
                        <Col md={8}>
                            <CountPart />
                        </Col>
                        <Col md={4}>
                            <Card bg="danger" text="light">
                                <Card.Header>{t('dashboard_current_balance')}</Card.Header>
                                <Card.Body style={{ textAlign: "center" }}>
                                    <Card.Text>
                                        <p className="fw-bold display-5 text-right">
                                            {invoiceContext.loadingBalance ?
                                                <Skeleton />
                                                :
                                                <>
                                                    <small>R$</small>{invoiceContext.balance}
                                                </>
                                            }
                                        </p>
                                        {networkContext.currentRole == UserRoleEnum.Seller &&
                                            <>
                                                {invoiceContext.loadingAvailableBalance ?
                                                    <Skeleton />
                                                    :
                                                    <span>{t('dashboard_amount_released_for_withdrawal')} <small>R$</small>{invoiceContext.availableBalance}</span>
                                                }
                                            </>
                                        }
                                    </Card.Text>
                                    <Button variant="danger" disabled>{t('dashboard_withdrawal')} <FontAwesomeIcon icon={faArrowRight} fixedWidth /></Button>
                                </Card.Body>
                            </Card>
                        </Col>
                    </Row>
                }
            </Container>
            <Container>
                <Row>
                    <Col md={networkContext.currentRole != UserRoleEnum.User ? 8 : 12}>
                        <Tabs
                            defaultActiveKey="balance"
                            id="uncontrolled-tab-example"
                            className="mb-3"
                        >
                            <Tab eventKey="balance" title={
                                <>
                                    <FontAwesomeIcon icon={faDollar} fixedWidth />&nbsp;{t('dashboard_statement')}
                                </>
                            }>
                                <StatementPart
                                    loading={invoiceContext.loadingSearch}
                                    StatementResult={invoiceContext.statementResult}
                                    onChangePage={(pagenum: number) => {
                                        searchStatements(pagenum);
                                    }} />
                            </Tab>
                            <Tab eventKey="order" title={t('dashboard_orders_tab')} disabled>
                                Tab content for Profile
                            </Tab>
                        </Tabs>

                    </Col>
                    {networkContext.currentRole != UserRoleEnum.User &&
                        <Col md={4} className="py-4">
                            <ListGroup>
                                {networkContext.currentRole >= UserRoleEnum.NetworkManager &&
                                    <>
                                        <ListGroup.Item variant="primary">
                                            <FontAwesomeIcon icon={faUserGroup} fixedWidth /> {t('dashboard_networks_title')}
                                        </ListGroup.Item>
                                        <ListGroup.Item action onClick={() => {
                                            navigate("/admin/network");
                                        }}>
                                            <div className="ms-2 me-auto">
                                                <FontAwesomeIcon icon={faCog} fixedWidth /> {t('preferences')}
                                            </div>
                                        </ListGroup.Item>
                                        <ListGroup.Item action onClick={() => {
                                            navigate("/admin/team-structure");
                                        }}>
                                            <div className="ms-2 me-auto">
                                                <FontAwesomeIcon icon={faUserCog} fixedWidth /> {t('team_structure')}
                                            </div>
                                        </ListGroup.Item>
                                        <ListGroup.Item action onClick={() => {
                                            navigate("/admin/teams");
                                        }}>
                                            {/*<Badge bg="primary" pill style={{ float: "right" }}>7</Badge>*/}
                                            <div className="ms-2 me-auto">
                                                <FontAwesomeIcon icon={faUserGroup} fixedWidth /> {t('teams')}
                                            </div>
                                        </ListGroup.Item>
                                    </>
                                }
                                {networkContext.currentRole >= UserRoleEnum.Seller &&
                                    <>
                                        <ListGroup.Item variant="primary">
                                            <FontAwesomeIcon icon={faBox} fixedWidth /> {t('finances')}
                                        </ListGroup.Item>
                                        <ListGroup.Item action onClick={() => {
                                            navigate("/admin/orders");
                                        }}>
                                            {/*<Badge bg="primary" pill style={{ float: "right" }}>7</Badge>*/}
                                            <div className="ms-2 me-auto">
                                                <FontAwesomeIcon icon={faList} fixedWidth /> {t('orders')}
                                            </div>
                                        </ListGroup.Item>
                                        <ListGroup.Item action onClick={() => {
                                            navigate("/admin/invoices");
                                        }}>
                                            {/*<Badge bg="primary" pill style={{ float: "right" }}>7</Badge>*/}
                                            <div className="ms-2 me-auto">
                                                <FontAwesomeIcon icon={faDollar} fixedWidth /> {t('invoices')}
                                            </div>
                                        </ListGroup.Item>
                                        <ListGroup.Item action onClick={() => {
                                            navigate("/admin/products");
                                        }}>
                                            {/*<Badge bg="primary" pill style={{ float: "right" }}>7</Badge>*/}
                                            <div className="ms-2 me-auto">
                                                <FontAwesomeIcon icon={faBox} fixedWidth /> {t('products')}
                                            </div>
                                        </ListGroup.Item>
                                    </>
                                }
                            </ListGroup>
                        </Col>
                    }
                </Row>
            </Container>
        </>
    );

}