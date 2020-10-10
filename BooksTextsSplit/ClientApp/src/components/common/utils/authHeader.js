export function authHeader(props) {
  /*     // code block FROM userService - start
    // return authorization header with basic auth credentials
    let user = JSON.parse(localStorage.getItem('user'));

    if (user && user.authdata) {
        return { 'Authorization': 'Basic ' + user.authdata };
    } else {
        return {};
    }
    // code block FROM userService - end */

  if (props.authKey) {
    return { Authorization: "Basic " + props.authKey };
  }
}
