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
import { faBitcoinSign, faCalendar, faCalendarAlt, faCancel, faClose, faEnvelope, faEthernet, faLock, faSave, faSignInAlt, faTrash, faUpload, faUser } from '@fortawesome/free-solid-svg-icons';
import Table from "react-bootstrap/esm/Table";
import { Link, useNavigate } from "react-router-dom";
import InputGroup from 'react-bootstrap/InputGroup';
import UserContext from "../../Contexts/User/UserContext";
import MessageToast from "../../Components/MessageToast";
import Moment from 'moment';
import { MessageToastEnum } from "../../DTO/Enum/MessageToastEnum";
import { ImageModal, ImageTypeEnum } from "../../Components/ImageModal";

export default function UserPage() {

    const { t } = useTranslation();

    const authContext = useContext(AuthContext);
    const userContext = useContext(UserContext);

    const [insertMode, setInsertMode] = useState<boolean>(false);
    const [showImageModal, setShowImageModal] = useState<boolean>(false);

    const [dialog, setDialog] = useState<MessageToastEnum>(MessageToastEnum.Error);
    const [showMessage, setShowMessage] = useState<boolean>(false);
    const [messageText, setMessageText] = useState<string>("");

    const [confirmPassword, setConfirmPassword] = useState<string>("");

    let navigate = useNavigate();
    Moment.locale('en');

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
        //alert(JSON.stringify(authContext.sessionInfo));
        if (authContext.sessionInfo) {
            if (authContext.sessionInfo?.userId > 0) {
                userContext.getMe().then((ret) => {
                    if (ret.sucesso) {
                        setInsertMode(false);
                    }
                    else {
                        setInsertMode(true);
                    }
                });
            }
            else {
                setInsertMode(true);
            }
        }
        else {
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
            <ImageModal
                Image={ImageTypeEnum.User}
                show={showImageModal}
                onClose={() => setShowImageModal(false)}
                onSuccess={(url: string) => {
                    userContext.setUser({
                        ...userContext.user,
                        imageUrl: url
                    });
                }}
            />
            <Container>
                <Card>
                    <Card.Body>
                        <Row>
                            <Col md="3" className="text-center">
                                <div className="mb-2">
                                    {userContext.user?.imageUrl &&
                                        <img src={userContext.user?.imageUrl}
                                            className="rounded-circle"
                                            style={{ width: "12rem", height: "12rem" }}
                                        />
                                    }
                                </div>
                                <div className="lc-block d-grid gap-3 d-md-block">
                                    {userContext.user?.userId > 0 &&
                                        <Button variant="primary" className="me-md-2" size="lg" onClick={async (e) => {
                                            e.preventDefault();
                                            setShowImageModal(true);
                                        }}>
                                            <FontAwesomeIcon icon={faUpload} fixedWidth />&nbsp;{t("userPage.changeImage")}
                                        </Button>
                                    }
                                </div>
                            </Col>
                            <Col md="9">
                                <Form>
                                    {/* <div className="text-center mb-3">
                                        {t("userPage.registrationNote")}
                                    </div> */}
                                    {!insertMode &&
                                        <Form.Group as={Row} className="mb-3">
                                            <Form.Label column sm="2">{t("userPage.hashLabel")}:</Form.Label>
                                            <Col sm="10">
                                                <InputGroup>
                                                    <InputGroup.Text><FontAwesomeIcon icon={faLock} fixedWidth /></InputGroup.Text>
                                                    <Form.Control type="text" size="lg" className="readonly"
                                                        disabled={true} readOnly={true}
                                                        value={userContext.user?.hash}
                                                    />
                                                </InputGroup>
                                            </Col>
                                        </Form.Group>
                                    }
                                    <Form.Group as={Row} className="mb-3">
                                        <Form.Label column sm="2">{t("userPage.nameLabel")}:</Form.Label>
                                        <Col sm="10">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faUser} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="text" size="lg"
                                                    placeholder={t("userPage.namePlaceholder")}
                                                    value={userContext.user?.name}
                                                    onChange={(e) => {
                                                        userContext.setUser({
                                                            ...userContext.user,
                                                            name: e.target.value
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                    </Form.Group>
                                    <Form.Group as={Row} className="mb-3">
                                        <Form.Label column sm="2">{t("userPage.emailLabel")}:</Form.Label>
                                        <Col sm="10">
                                            <InputGroup>
                                                <InputGroup.Text><FontAwesomeIcon icon={faEnvelope} fixedWidth /></InputGroup.Text>
                                                <Form.Control type="text" size="lg"
                                                    placeholder={t("userPage.emailPlaceholder")}
                                                    value={userContext.user?.email}
                                                    onChange={(e) => {
                                                        userContext.setUser({
                                                            ...userContext.user,
                                                            email: e.target.value
                                                        });
                                                    }} />
                                            </InputGroup>
                                        </Col>
                                    </Form.Group>
                                    {insertMode &&
                                        <>
                                            <Form.Group as={Row} className="mb-3">
                                                <Form.Label column sm="2">{t("userPage.passwordLabel")}:</Form.Label>
                                                <Col sm="10">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faLock} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="password" size="lg"
                                                            placeholder={t("userPage.passwordPlaceholder")}
                                                            value={userContext.user?.password}
                                                            onChange={(e) => {
                                                                userContext.setUser({
                                                                    ...userContext.user,
                                                                    password: e.target.value
                                                                });
                                                            }} />
                                                    </InputGroup>
                                                </Col>
                                            </Form.Group>
                                            <Form.Group as={Row} className="mb-3">
                                                <Form.Label column sm="2">{t("userPage.confirmPasswordLabel")}:</Form.Label>
                                                <Col sm="10">
                                                    <InputGroup>
                                                        <InputGroup.Text><FontAwesomeIcon icon={faLock} fixedWidth /></InputGroup.Text>
                                                        <Form.Control type="password" size="lg"
                                                            placeholder={t("userPage.confirmPasswordPlaceholder")}
                                                            value={confirmPassword}
                                                            onChange={(e) => {
                                                                setConfirmPassword(e.target.value);
                                                            }} />
                                                    </InputGroup>
                                                </Col>
                                            </Form.Group>
                                        </>
                                    }
                                    {!insertMode &&
                                        <Form.Group as={Row} className="mb-3">
                                            <Form.Label column sm="2">{t("userPage.createdAtLabel")}:</Form.Label>
                                            <Col sm="4">
                                                <InputGroup>
                                                    <InputGroup.Text><FontAwesomeIcon icon={faCalendarAlt} fixedWidth /></InputGroup.Text>
                                                    <Form.Control type="text" size="lg" disabled={true} readOnly={true}
                                                        value={userContext.user?.createAt ? Moment(userContext.user?.createAt).format("DD/MM/YYYY") : ""} />
                                                </InputGroup>
                                            </Col>
                                            <Form.Label column sm="2">{t("userPage.updatedAtLabel")}:</Form.Label>
                                            <Col sm="4">
                                                <InputGroup>
                                                    <InputGroup.Text><FontAwesomeIcon icon={faCalendarAlt} fixedWidth /></InputGroup.Text>
                                                    <Form.Control type="text" size="lg" disabled={true} readOnly={true}
                                                        value={userContext.user?.updateAt ? Moment(userContext.user?.updateAt).format("DD/MM/YYYY") : ""} />
                                                </InputGroup>
                                            </Col>
                                        </Form.Group>
                                    }
                                    <div className="d-grid gap-2 d-md-flex justify-content-md-end">
                                        {/* <Button variant="danger" size="lg" onClick={() => {
                                            navigate("/account/login");
                                        }}><FontAwesomeIcon icon={faSignInAlt} fixedWidth /> {t("userPage.signIn")}</Button> */}
                                        <Button variant="success" size="lg" onClick={async (e) => {
                                            if (insertMode) {
                                                if (userContext.user?.password != confirmPassword) {
                                                    throwError(t("userPage.errors.passwordsNotEqual"));
                                                    return;
                                                }
                                                let ret = await userContext.insert(userContext.user);
                                                if (ret.sucesso) {
                                                    showSuccessMessage(ret.mensagemSucesso);
                                                    //alert(userContext.user?.id);
                                                }
                                                else {
                                                    throwError(ret.mensagemErro);
                                                }
                                            }
                                            else {
                                                let ret = await userContext.update(userContext.user);
                                                if (ret.sucesso) {
                                                    //alert(userContext.user?.id);
                                                    showSuccessMessage(ret.mensagemSucesso);
                                                }
                                                else {
                                                    throwError(ret.mensagemErro);
                                                }
                                            }
                                        }}
                                            disabled={userContext.loadingUpdate}
                                        ><FontAwesomeIcon icon={faSave} fixedWidth /> {userContext.loadingUpdate ? t("loading") : t("buttons.save")}
                                        </Button>
                                    </div>
                                </Form>
                            </Col>
                        </Row>
                    </Card.Body>
                </Card>
            </Container>
        </>
    );
}