import React from "react";
import Footer from "./Footer";
import { connect } from "react-redux";
import { logout } from "../../redux/auth-reducer";
class FooterContainer extends React.Component {
  render() {
    return <Footer {...this.props} />;
  }
}

let mapStateToProps = (state) => ({
  isAuth: state.auth.isAuth,
  email: state.auth.email,
  login: state.auth.login,
});

export default connect(mapStateToProps, { logout })(FooterContainer);
