import React from "react";
import loadingImg from "../../../assets/images/tenor.gif";
import loadingImgAlt from "../../../assets/images/9aed57e18f39cc8eedd7128e1eec08e6.gif";

let Preloader = (props) => {
  return (
    <div>
      <img src={loadingImg} alt={loadingImgAlt} />
    </div>
  );
};

export default Preloader;
