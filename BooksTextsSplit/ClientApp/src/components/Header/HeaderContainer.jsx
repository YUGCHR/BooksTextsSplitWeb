import React from "react";
import Header from "./Header";
import { connect } from "react-redux";
import { logout } from "../../redux/auth-reducer";
class HeaderContainer extends React.Component {
  render() {
    return <Header {...this.props} />;
  }
}

let mapStateToProps = (state) => ({
  isAuth: state.auth.isAuth,
  email: state.auth.email,
  login: state.auth.login,
  isFetching: state.app.isFetching,
  whoCalled: state.app.whoCalledPreloader,
});

export default connect(mapStateToProps, { logout })(HeaderContainer);
