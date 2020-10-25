import React from "react";
import RadioButtons from "./RadioButtons";
import ShowFileDescriptions from "./ShowFileDescriptions";
import s from "./UploadBooks.module.css";

const ShowFiles = ({ chosenFiles, setRadioResult, radioChosenLanguage, filesDescriptions }) => {
  let radioInitialUniqValues = [
    { value: "eng", disabled: false, text: "English" },
    { value: "rus", disabled: null, text: "Russian" },
    { value: "other", disabled: true, text: "Auto select" },
  ];
  let radioInitialCommonValues = {
    placeholder: null,
    component: "Input",
    validators: [],
    type: "radio",
    uniqFormName: [], // put (formBaseName + i) here
  };
  let formBaseName = "radioForm";
  //let value="eng";
  // TODO сделать описания и значения отдельными колонками
  // TODO выводить каждое в одном блоке flex/grid
  // TODO сделать разные цвета для строк, чтобы была контрастная таблица
  // TODO передавать значение количества строк в CSS
  // TODO вынести функцию отдельно и потом в отдельный файл - partially done
  return Array.from(chosenFiles).map((f, i) => {
    radioInitialCommonValues.uniqFormName[i] = formBaseName + i;
    //console.log("uniqFormName" + i, radioInitialCommonValues.uniqFormName[i]);
    return (
      <div>
        <div>
          <ShowFileDescriptions radioChosenLanguage={radioChosenLanguage} f={f} i={i} filesDescriptions={filesDescriptions} />
        </div>
        <div>
          <RadioButtons
            uV={radioInitialUniqValues}
            cV={radioInitialCommonValues}
            i={i}
            setRadioResult={setRadioResult}
            radioChosenLanguage={radioChosenLanguage}
          />
        </div>
      </div>
    );
  });
};

export default ShowFiles;
