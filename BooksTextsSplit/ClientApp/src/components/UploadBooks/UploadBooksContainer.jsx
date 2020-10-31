import React from "react";
import { compose } from "redux";
import { connect } from "react-redux";
import { withAuthRedirect } from "../../hoc/withAuthRedirect";
import {
  fetchSentencesCount,
  fileUploadHandler,
  setFilesNamesAndEnableUpload,
  setRadioResult,
  setShowHideState,
} from "../../redux/upload-reducer";
import UploadBooks from "./UploadBooks";
import Preloader from "../common/preloader/Preloader";

class UploadBooksContainerAPI extends React.Component {
  componentDidMount() {
    this.props.setFilesNamesAndEnableUpload(null);
    this.props.fetchSentencesCount(0);
    this.props.fetchSentencesCount(1);
  }
  /* TODO it is possible to give the place (className style) where preloader will be shown */
  render() {
    return (
      <>
        {this.props.isFetching ? <Preloader /> : null}
        <UploadBooks
          selectedFiles={this.props.selectedFiles} // used in ShowSelectedFiles
          radioChosenLanguage={this.props.radioChosenLanguage} // used in ShowSelectedFiles
          setRadioResult={this.props.setRadioResult} // used in ShowSelectedFiles
          selectedRadioLanguage={this.props.selectedRadioLanguage}
          filesDescriptions={this.props.filesDescriptions} //
          labelShowHide={this.props.labelShowHide} //
          setShowHideState={this.props.setShowHideState} //
          uploadBooksLabels={this.props.uploadBooksLabels} //
          isDoneUpload={this.props.isDoneUpload} //
          isUploadButtonDisabled={this.props.isUploadButtonDisabled} //
          filesLanguageIds={this.props.filesLanguageIds}
          booksTitles={this.props.booksTitles}
          sentencesCount={this.props.sentencesCount}
          setFileName={this.props.setFilesNamesAndEnableUpload} // TODO rename to make all names identical
          radioOptionChange={this.props.radioOptionChange} //
          fileUploadHandler={this.props.fileUploadHandler} //
          isWrongCount={this.props.isWrongCount}
          dbSentencesCount={this.props.dbSentencesCount}
        />
      </>
    );
  }
}

let mapStateToProps = (state) => {
  return {
    selectedFiles: state.uploadBooksPage.selectedFiles, //
    radioChosenLanguage: state.uploadBooksPage.radioChosenLanguage, //
    selectedRadioLanguage: state.uploadBooksPage.selectedRadioLanguage, //
    filesDescriptions: state.uploadBooksPage.filesDescriptions, //
    radioButtonsLabels: state.uploadBooksPage.radioButtonsLabels, //
    radioButtonsNames: state.uploadBooksPage.radioButtonsNames,
    radioButtonsValues: state.uploadBooksPage.radioButtonsValues,
    radioButtonsIds: state.uploadBooksPage.radioButtonsIds, //
    filesLanguageIds: state.uploadBooksPage.filesLanguageIds, //
    labelShowHide: state.uploadBooksPage.labelShowHide, //
    uploadBooksLabels: state.uploadBooksPage.uploadBooksLabels, //
    isDoneUpload: state.uploadBooksPage.isDoneUpload, //
    isUploadButtonDisabled: state.uploadBooksPage.isUploadButtonDisabled, //
    isWrongCount: state.uploadBooksPage.isWrongCount, //
    booksTitles: state.uploadBooksPage.booksTitles,
    dbSentencesCount: state.uploadBooksPage.dbSentencesCount,
    sentencesCount: state.uploadBooksPage.sentencesCount,
    isFetching: state.uploadBooksPage.isFetching,
  };
};

let UploadBooksContainer = compose(
  connect(mapStateToProps, {
    fetchSentencesCount,
    fileUploadHandler,
    setFilesNamesAndEnableUpload,
    setRadioResult,
    setShowHideState,
  }),
  withAuthRedirect
)(UploadBooksContainerAPI);

export default UploadBooksContainer;
