//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2025-05-22 15:18:13
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 邮件协议
/// </summary>
public struct MailProto : IProto
{
    public ushort ProtoCode { get { return 18001; } }

    public int Id; //编号
    public bool IsSuccess; //是否成功
    public int ErrorCode; //错误编号
    public string content; //邮件内容

    public byte[] ToArray()
    {
        using (MMO_MemoryStream ms = new MMO_MemoryStream())
        {
            ms.WriteUShort(ProtoCode);
            ms.WriteInt(Id);
            ms.WriteBool(IsSuccess);
            if(!IsSuccess)
            {
            }
            ms.WriteInt(ErrorCode);
            ms.WriteUTF8String(content);
            return ms.ToArray();
        }
    }

    public static MailProto GetProto(byte[] buffer)
    {
        MailProto proto = new MailProto();
        using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
        {
            proto.Id = ms.ReadInt();
            proto.IsSuccess = ms.ReadBool();
            if(!proto.IsSuccess)
            {
            }
            proto.ErrorCode = ms.ReadInt();
            proto.content = ms.ReadUTF8String();
        }
        return proto;
    }
}