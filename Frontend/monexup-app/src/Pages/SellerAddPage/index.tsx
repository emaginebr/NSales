import { useContext, useEffect, useState } from "react";
import Col from "react-bootstrap/esm/Col";
import { useTranslation } from "react-i18next";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import Form from 'react-bootstrap/Form';
import AuthContext from "../../Contexts/Auth/AuthContext";
import Button from "react-bootstrap/esm/Button";
import Card from 'react-bootstrap/Card';
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faAddressBook, faArrowLeft, faBitcoinSign, faCalendar, faCalendarAlt, faCancel, faClose, faDollar, faEnvelope, faEthernet, faIdCard, faLock, faPhone, faSave, faSignInAlt, faTrash, faUser } from '@fortawesome/free-solid-svg-icons';
import Table from "react-bootstrap/esm/Table";
import { Link, useNavigate, useParams } from "react-router-dom";
import InputGroup from 'react-bootstrap/InputGroup';
import UserContext from "../../Contexts/User/UserContext";
import MessageToast from "../../Components/MessageToast";
import Moment from 'moment';
import { MessageToastEnum } from "../../DTO/Enum/MessageToastEnum";
import UserEditInfo from "../../DTO/Domain/UserEditInfo";
import UserInfo from "../../DTO/Domain/UserInfo";
import UserPhoneInfo from "../../DTO/Domain/UserPhoneInfo";
import UserAddressInfo from "../../DTO/Domain/UserAddressInfo";
import NetworkFooter from "../NetworkPage/NetworkFooter";
import Footer from "../HomePage/Footer";

export default function SellerAddPage() {

    const { t } = useTranslation();

    const authContext = useContext(AuthContext);
    const userContext = useContext(UserContext);

    const [insertMode, setInsertMode] = useState<boolean>(false);

    const [user, setUser] = useState<UserEditInfo>(null);

    const [dialog, setDialog] = useState<MessageToastEnum>(MessageToastEnum.Error);
    const [showMessage, setShowMessage] = useState<boolean>(false);
    const [messageText, setMessageText] = useState<string>("");

    let { networkSlug } = useParams();

    let navigate = useNavigate();
    //Moment.locale('en');

    const throwError = (message: string) => {
        setDialog(MessageToastEnum.Error)
        setMessageText(message);
        setShowMessage(true);
    };
    const showSuccessMessage = (message: string) => {
        setDialog(MessageToastEnum.Success)
        setMessageText(message);
        setShowMessage(true);
    };

    useEffect(() => {
        let userEdit: UserEditInfo;
        if (authContext.sessionInfo) {
            if (authContext.sessionInfo?.userId > 0) {
                userContext.getMe().then((ret) => {
                    if (ret.sucesso) {
                        setUser({
                            ...userEdit,
                            userId: ret.user.userId,
                            name: ret.user.name,
                            email: ret.user.email,
                            birthDate: ret.user.birthDate,
                            iddocument: ret.user.idDocument,
                            pixkey: ret.user.pixKey,
                            phone: ret.user.phones[0]?.phone,
                            zipCode: ret.user.addresses[0]?.zipCode,
                            address: ret.user.addresses[0]?.address,
                            complement: ret.user.addresses[0]?.complement,
                            neighborhood: ret.user.addresses[0]?.neighborhood,
                            city: ret.user.addresses[0]?.city,
                            state: ret.user.addresses[0]?.state
                        });

                        setInsertMode(false);
                    }
                    else {
                        setUser(userEdit);
                        setInsertMode(true);
                    }
                });
            }
            else {
                setUser(userEdit);
                setInsertMode(true);
            }
        }
        else {
            setUser(userEdit);
            setInsertMode(true);
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
            <Container className="mb-5">
                <Row>
                    <Col md="12">
                        <Card>
                            <Card.Header>
                                <h3 className="text-center">{t("sellerAddPage.title")}</h3>
                            </Card.Header>
                            <Card.Body>
                                <Form>
                                    <div className="text-center mb-3">
                                        {t("sellerAddPage.registrationNote")}
                                    </div>
                                    <Form.Group as={Row} className="mb-3">
                                        <Form.Label column sm="2">{t("sellerAddPage.nameLabel")}:</Form.Label>
                                        <Col sm="10">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faUser} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="text" size="lg"
                                                    placeholder={t("sellerAddPage.namePlaceholder")}
                                                    value={user?.name}
                                                    onChange={(e) => {
                                                        setUser({
                                                            ...user,
                                                            name: e.target.value
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                    </Form.Group>
                                    <Form.Group as={Row} className="mb-3">
                                        <Form.Label column sm="2">{t("sellerAddPage.cpfLabel")}:</Form.Label>
                                        <Col sm="5">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faIdCard} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="text" size="lg"
                                                    placeholder={t("sellerAddPage.cpfPlaceholder")}
                                                    value={user?.iddocument}
                                                    onChange={(e) => {
                                                        setUser({
                                                            ...user,
                                                            iddocument: e.target.value
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                        <Form.Label column sm="1">{t("sellerAddPage.birthdayLabel")}:</Form.Label>
                                        <Col sm="4">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faCalendar} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="date" size="lg"
                                                    placeholder={t("sellerAddPage.birthdayPlaceholder")}
                                                    value={user?.birthDate}
                                                    onChange={(e) => {
                                                        setUser({
                                                            ...user,
                                                            birthDate: e.target.value
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                    </Form.Group>
                                    <Form.Group as={Row} className="mb-3">
                                        <Form.Label column sm="2">{t("sellerAddPage.emailLabel")}:</Form.Label>
                                        <Col sm="5">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faEnvelope} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="email" size="lg"
                                                    placeholder={t("sellerAddPage.emailPlaceholder")}
                                                    value={user?.email}
                                                    onChange={(e) => {
                                                        setUser({
                                                            ...user,
                                                            email: e.target.value
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                        <Form.Label column sm="1">{t("sellerAddPage.phoneLabel")}:</Form.Label>
                                        <Col sm="4">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faPhone} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="text" size="lg"
                                                    placeholder={t("sellerAddPage.phonePlaceholder")}
                                                    value={user?.phone}
                                                    onChange={(e) => {
                                                        setUser({
                                                            ...user,
                                                            phone: e.target.value
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                    </Form.Group>
                                    <hr />
                                    <Form.Group as={Row} className="mb-3">
                                        <Form.Label column sm="2">{t("sellerAddPage.zipCodeLabel")}:</Form.Label>
                                        <Col sm="3">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faIdCard} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="text" size="lg"
                                                    placeholder={t("sellerAddPage.zipCodePlaceholder")}
                                                    value={user?.zipCode}
                                                    onChange={(e) => {
                                                        setUser({
                                                            ...user,
                                                            zipCode: e.target.value
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                        <Form.Label column sm="1">{t("sellerAddPage.addressLabel")}:</Form.Label>
                                        <Col sm="6">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faAddressBook} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="text" size="lg"
                                                    placeholder={t("sellerAddPage.addressPlaceholder")}
                                                    value={user?.address}
                                                    onChange={(e) => {
                                                        setUser({
                                                            ...user,
                                                            address: e.target.value
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                    </Form.Group>
                                    <Form.Group as={Row} className="mb-3">
                                        <Form.Label column sm="2">{t("sellerAddPage.complementLabel")}:</Form.Label>
                                        <Col sm="4">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faAddressBook} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="text" size="lg"
                                                    placeholder={t("sellerAddPage.complementPlaceholder")}
                                                    value={user?.complement}
                                                    onChange={(e) => {
                                                        setUser({
                                                            ...user,
                                                            complement: e.target.value
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                        <Form.Label column sm="2">{t("sellerAddPage.neighborhoodLabel")}:</Form.Label>
                                        <Col sm="4">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faAddressBook} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="text" size="lg"
                                                    placeholder={t("sellerAddPage.neighborhoodPlaceholder")}
                                                    value={user?.neighborhood}
                                                    onChange={(e) => {
                                                        setUser({
                                                            ...user,
                                                            neighborhood: e.target.value
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                    </Form.Group>
                                    <Form.Group as={Row} className="mb-3">
                                        <Form.Label column sm="2">{t("sellerAddPage.cityLabel")}:</Form.Label>
                                        <Col sm="5">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faAddressBook} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="text" size="lg"
                                                    placeholder={t("sellerAddPage.cityPlaceholder")}
                                                    value={user?.city}
                                                    onChange={(e) => {
                                                        setUser({
                                                            ...user,
                                                            city: e.target.value
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                        <Form.Label column sm="1">{t("sellerAddPage.stateLabel")}:</Form.Label>
                                        <Col sm="4">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faAddressBook} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="text" size="lg"
                                                    placeholder={t("sellerAddPage.statePlaceholder")}
                                                    value={user?.state}
                                                    onChange={(e) => {
                                                        setUser({
                                                            ...user,
                                                            state: e.target.value
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                    </Form.Group>
                                    <hr />
                                    <Form.Group as={Row} className="mb-3">
                                        <Form.Label column sm="2">{t("sellerAddPage.pixKeyLabel")}:</Form.Label>
                                        <Col sm="10">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faDollar} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="text" size="lg"
                                                    placeholder={t("sellerAddPage.pixKeyPlaceholder")}
                                                    value={user?.pixkey}
                                                    onChange={(e) => {
                                                        setUser({
                                                            ...user,
                                                            pixkey: e.target.value
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                    </Form.Group>
                                    {insertMode &&
                                        <>
                                            <hr />
                                            <Form.Group as={Row} className="mb-3">
                                                <Form.Label column sm="2">{t("sellerAddPage.passwordLabel")}:</Form.Label>
                                                <Col sm="4">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faLock} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="password" size="lg"
                                                            placeholder={t("sellerAddPage.passwordPlaceholder")}
                                                            value={user?.password}
                                                            onChange={(e) => {
                                                                setUser({
                                                                    ...user,
                                                                    password: e.target.value
                                                                });
                                                            }} />
                                                    </InputGroup>
                                                </Col>
                                                <Form.Label column sm="2">{t("sellerAddPage.confirmPasswordLabel")}:</Form.Label>
                                                <Col sm="4">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faLock} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="password" size="lg"
                                                            placeholder={t("sellerAddPage.confirmPasswordPlaceholder")}
                                                            value={user?.confirmPassword}
                                                            onChange={(e) => {
                                                                setUser({
                                                                    ...user,
                                                                    confirmPassword: e.target.value
                                                                });
                                                            }} />
                                                    </InputGroup>
                                                </Col>
                                            </Form.Group>
                                        </>
                                    }
                                    <hr />
                                    {/* <Form.Group as={Row} className="mb-3">
                                        <Form.Label column sm="2">{t("sellerAddPage.uploadDocumentLabel")}:</Form.Label>
                                        <Col sm="10">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faAddressBook} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="file" size="lg" placeholder={t("sellerAddPage.uploadDocumentPlaceholder")} />
                                            </InputGroup>
                                        </Col>
                                    </Form.Group> */}
                                    <div className="d-grid gap-2 d-md-flex justify-content-md-end">
                                        <Button variant="danger" size="lg" onClick={() => {
                                            navigate(networkSlug ? "/" + networkSlug : "/");
                                        }}><FontAwesomeIcon icon={faArrowLeft} fixedWidth /> {t("buttons.back")}</Button>
                                        <Button variant="success" size="lg" onClick={async (e) => {
                                            e.preventDefault();

                                            if (!user.password) {
                                                throwError(t("sellerAddPage.errors.passwordEmpty"));
                                                return;
                                            }
                                            if (!user.confirmPassword) {
                                                throwError(t("sellerAddPage.errors.confirmPasswordEmpty"));
                                                return;
                                            }
                                            if (user.password != user.confirmPassword) {
                                                throwError(t("sellerAddPage.errors.passwordsNotEqual"));
                                                return;
                                            }

                                            let userFull: UserInfo;
                                            let userPhone: UserPhoneInfo;
                                            let userAddr: UserAddressInfo;

                                            userFull = {
                                                ...userFull,
                                                userId: user.userId,
                                                name: user.name,
                                                email: user.email,
                                                idDocument: user.iddocument,
                                                birthDate: user.birthDate,
                                                pixKey: user.pixkey,
                                                password: user.password,
                                                phones: [],
                                                addresses: []
                                            };
                                            userFull.phones.push({
                                                ...userPhone,
                                                phone: user.phone,
                                            });
                                            userFull.addresses.push({
                                                ...userAddr,
                                                zipCode: user.zipCode,
                                                address: user.address,
                                                complement: user.complement,
                                                neighborhood: user.neighborhood,
                                                city: user.city,
                                                state: user.state
                                            });


                                            if (insertMode) {
                                                let ret = await userContext.insert(userFull);
                                                if (ret.sucesso) {
                                                    showSuccessMessage(ret.mensagemSucesso);
                                                }
                                                else {
                                                    throwError(ret.mensagemErro);
                                                }
                                            }
                                            else {
                                                let ret = await userContext.update(userFull);
                                                if (ret.sucesso) {
                                                    showSuccessMessage(ret.mensagemSucesso);
                                                }
                                                else {
                                                    throwError(ret.mensagemErro);
                                                }
                                            }
                                        }}
                                            disabled={userContext.loadingUpdate}
                                        ><FontAwesomeIcon icon={faSave} fixedWidth />
                                            {userContext.loadingUpdate ? t("loading") : t("buttons.save")}
                                        </Button>
                                    </div>
                                </Form>
                            </Card.Body>
                        </Card>
                    </Col>
                </Row>
            </Container>
        </>
    );
}