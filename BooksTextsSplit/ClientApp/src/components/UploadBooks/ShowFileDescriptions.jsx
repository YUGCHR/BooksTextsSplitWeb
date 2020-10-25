import React from "react";
import s from "./UploadBooks.module.css";

const ShowFileDescriptions = ({ f, i, radioChosenLanguage, filesDescriptions }) => {
  let newDateFormat = (inputDate) => {
    return new Intl.DateTimeFormat("en-GB", {
      year: "numeric",
      month: "long",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    }).format(inputDate);
  };

  return (
    <div>
      <div>{filesDescriptions.index + " " + i} </div>
      <div>{filesDescriptions.name + " " + f.name}</div>
      {/* <div>Last modified: {" " + f.lastModifiedDate.toLocaleDateString()}</div> */}
      <div>{filesDescriptions.lastMod + " " + newDateFormat(f.lastModifiedDate)}</div>
      <div>{filesDescriptions.size + " " + Math.round(f.size / 1024) + " KB"}</div>
      <div>{filesDescriptions.type + " " + f.type}</div>
      <div className={s.chosenLanguagePlace}>
        <div className={s.chosenLanguageText}>{filesDescriptions.chosenLanguage}</div>
        <div className={s.chosenLanguage}>{" " + radioChosenLanguage[i]}</div>
      </div>
    </div>
  );
};

export default ShowFileDescriptions;
