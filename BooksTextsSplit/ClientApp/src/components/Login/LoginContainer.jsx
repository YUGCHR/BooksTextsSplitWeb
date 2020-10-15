import React from 'react';
import Header from './Header';
import { connect } from 'react-redux';
import { getAuthUserData } from '../../redux/auth-reducer';
import store from '../../redux/redux-store';
import { withRouter } from 'react-router-dom';

class LoginContainer extends React.Component {

  componentDidMount() {
    this.props.getAuthUserData(); //thunk
  }

  render() {
    return (
      <Header {...this.props} />
    );
  }
}

let mapStateToProps = (state) => ({
  isAuth: state.auth.isAuth,
  login: state.auth.login
})

export default connect(mapStateToProps, { getAuthUserData })(LoginContainer);
