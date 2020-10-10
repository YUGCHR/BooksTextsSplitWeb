import React from 'react';
import { connect } from 'react-redux';
import { getAuthUserData } from '../../redux/auth-reducer';

export function authHeader({authKey}) {
  /*     // code block FROM userService - start
    // return authorization header with basic auth credentials
    let user = JSON.parse(localStorage.getItem('user'));

    if (user && user.authdata) {
        return { 'Authorization': 'Basic ' + user.authdata };
    } else {
        return {};
    }
    // code block FROM userService - end */

  if (authKey) {
    return { Authorization: "Basic " + authKey };
  }
}
class AuthHeaderContainer extends React.Component {

  /* componentDidMount() {
    this.props.getAuthUserData(); //thunk
  } */

  render() {
    return (
      <authHeader {authKey} />
    );
  }
}

let mapStateToProps = (state) => ({
  isAuth: state.auth.isAuth,
  authKey: state.auth.authKey
})

export default connect(mapStateToProps, { getAuthUserData })(AuthHeaderContainer);

