import React from "react";

export const ShowFilesToUpload = ({ selectedFiles, sentencesCount }) => {
  // TODO принести сюда правильный languageId
  return Array.from(selectedFiles).map((sf, n) => {
    return (
      <div>
        {/* <div> File To Upload No: {n} </div> */}
        <div>File name: {" " + sf.name}</div>
        {/* <div>File language: {" " + sf.languageId}</div>
        <div>Uploaded sentences count: {" " + sentencesCount[n]}</div> */}
      </div>
    );
  });
};
