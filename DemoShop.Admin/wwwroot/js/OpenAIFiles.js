const app = Vue.createApp({
        components: {
            EasyDataTable: window['vue3-easy-data-table'],
        },
        data() {
            return {
                loading: false,
                headers: [
                    {text: "Id", value: "id", sortable: true},
                    {text: "Purpose", value: "purpose"},
                    {text: "Filename", value: "filename"},
                    {text: "Bytes", value: "bytes"},
                    {text: "Status", value: "status"},
                    {text: "CreatedAt", value: "created_at"},
                ],
                files: [
                    // {
                    //     "object": "file",
                    //     "id": "file-TlfLjdOW9WXBfiIcxm67Z5oi",
                    //     "purpose": "assistants",
                    //     "filename": "查核新聞-prompt 範例.md",
                    //     "bytes": 4388,
                    //     "created_at": 1712549886,
                    //     "status": "processed",
                    //     "status_details": null
                    // }
                ],
                newFile: null,
                newFileValidateState: false,
                newFileErrorMsg: "",
                newFileModal: null,
            };
        },
        methods: {
            uploadFileModal() {
                this.newFile = null;
                this.newFileModal.show();
            },
            onFileChange(event) {
                console.log(event.target.files);
                this.newFile = event.target.files[0];
                this.newFileValidateState = this.validateFileType(this.newFile);
            },
            validateFileType(file) {
                // 允許的文件類型
                const allowedTypes = ['application/json', 'application/pdf', 'text/markdown', 'text/plain', 'image/png', 'image/jpeg'];
                // 允許的最大文件大小（10MB）
                const maxSize = 10 * 1024 * 1024; // 10MB

                // 檢查文件的MIME類型是否在允許的類型列表中
                if (allowedTypes.includes(file.type)) {
                    // 檢查文件大小是否超過最大限制
                    if (file.size <= maxSize) {
                        // 如果文件類型和大小都符合要求，返回true
                        this.newFileErrorMsg = '';
                        return true;
                    } else {
                        // 如果文件大小超過限制，設置錯誤消息並返回false
                        this.newFileErrorMsg = '文件大小不能超過10MB';
                        return false;
                    }
                } else {
                    // 如果文件類型不允許，設置錯誤消息並返回false
                    this.newFileErrorMsg = '只允許上傳一般文檔、PDF或JPEG、PNG格式的文件';
                    return false;
                }
            },
            uploadNewFile() {
                this.loading = true;
                const formData = new FormData();
                formData.append('file', this.newFile);
                httpPost(`/api/FileUpload/UploadFileToOpenAI`, formData)
                    .then(response => {
                        if (response.isSuccess) {
                            this.newFileModal.hide();
                        }
                    })
                    .catch(err => {
                        console.error(err)
                    })
                    .finally(() => {
                        this.newFile = null;
                        this.loading = false;
                        this.getAllFiles();
                    })
            },
            getAllFiles() {
                this.loading = true;
                httpGet(`/api/FileUpload/GetAllFilesFromOpenAI`)
                    .then(response => {
                        if (response.isSuccess) {
                            const files = JSON.parse(response.body);
                            this.files = files.data;
                        }
                    })
                    .catch(err => {
                        console.error(err);
                    })
                    .finally(() => {
                        this.loading = false;
                    });
            },
            copyToClipboard(text) {
                navigator.clipboard.writeText(text)
            }
        },
        mounted() {
            this.newFileModal = new bootstrap.Modal(this.$refs.fileUploadModal, {
                keyboard: false
            })
            this.getAllFiles()
        },
        computed: {},
    })
;

app.mount("#app");

